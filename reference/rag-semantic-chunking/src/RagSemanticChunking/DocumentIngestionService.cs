using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;

namespace RagSemanticChunking;

/// <summary>Orquesta carga, chunking, embeddings finales y publicación para cualquier estrategia.</summary>
/// <remarks>La estrategia sólo decide los cortes; el resto del pipeline se comparte para comparar de forma consistente.</remarks>
public sealed class DocumentIngestionService(IEmbeddingGenerator<string, Embedding<float>> generator,
    VectorStore store, ChunkingSettings settings, ILogger<DocumentIngestionService> logger,
    string collectionPrefix = ExperimentSettings.CollectionPrefix)
{
    /// <summary>Carga archivos .md y .txt del directorio, sin recorrer subdirectorios.</summary>
    /// <remarks>El orden ordinal estabiliza hashes y recorridos; no aumenta la relevancia semántica de un archivo.</remarks>
    public static async Task<IReadOnlyList<DocumentText>> LoadCorpusAsync(string directory, CancellationToken token)
    {
        string[] paths = Directory.EnumerateFiles(directory).Where(p => Path.GetExtension(p).ToLowerInvariant() is ".md" or ".txt")
            .Order(StringComparer.Ordinal).ToArray();
        List<DocumentText> documents = [];
        foreach (string path in paths)
        {
            DocumentText document = await DocumentText.LoadAsync(path, token);
            if (document.Text.Length == 0) throw new InvalidOperationException($"Documento vacío: {document.Source}.");
            documents.Add(document);
        }
        if (documents.Count == 0) throw new InvalidOperationException("data/ no contiene documentos.");
        return documents;
    }

    /// <summary>Prepara una ingesta completa y publica registros, eliminaciones y manifiesto.</summary>
    /// <param name="directory">Directorio local del corpus.</param>
    /// <param name="artifacts">Directorio de snapshots y bloqueo del escritor.</param>
    /// <param name="strategy">Única parte variable del pipeline: cómo construir chunks.</param>
    /// <param name="config">Identidad del proveedor y modelos que quedará vinculada a la ingesta.</param>
    /// <param name="token">Cancelación propagada a archivos, IA y Qdrant.</param>
    /// <returns>Snapshot de la generación publicada.</returns>
    /// <remarks>La publicación no es transaccional; no se deben consultar colecciones durante su actualización.</remarks>
    public async Task<IndexManifest> IngestAsync(string directory, string artifacts, IChunkingStrategy strategy,
        AppConfiguration config, CancellationToken token)
    {
        settings.Validate();
        Directory.CreateDirectory(artifacts);
        // Un escritor por referencia evita publicaciones simultáneas desde esta misma carpeta.
        // FileShare.None bloquea otro escritor que use esta carpeta; no es un lock distribuido entre máquinas.
        using FileStream writerLock = new(Path.Combine(artifacts, "ingest.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        string collectionName = ExperimentSettings.Collection(strategy.Name, collectionPrefix);
        bool exists = await store.CollectionExistsAsync(collectionName, token);
        IndexManifest? previous = File.Exists(IndexManifest.PathFor(artifacts, strategy.Name))
            ? await IndexManifest.LoadAsync(artifacts, strategy.Name, token) : null;
        // Sin el snapshot anterior desconoceríamos los IDs sobrantes. Abortamos para no dejar
        // silenciosamente fragmentos de ingestas previas mezclados con los nuevos.
        if (exists && previous is null) throw new InvalidOperationException("Existe la colección pero falta su manifiesto. Recuperá artifacts o limpiá sólo esta colección deliberadamente.");
        previous?.ValidateConfiguration(config);
        IReadOnlyList<DocumentText> documents = await LoadCorpusAsync(directory, token);
        List<DocumentRecord> records = [];
        List<DocumentChunk> allChunks = [];
        List<DocumentMetrics> metrics = [];
        // La generación permite detectar resultados que no pertenecen al snapshot que está usando el lector.
        string generation = Guid.NewGuid().ToString("D");
        int? dimensions = previous?.Dimensions;
        foreach (DocumentText document in documents)
        {
            // Medimos la decisión de corte por separado del coste común de vectorizar y persistir los chunks.
            Stopwatch watch = Stopwatch.StartNew();
            ChunkingResult result = await strategy.ChunkAsync(document, token);
            watch.Stop();
            TextPartition.ValidateCoverage(document, result.Chunks, settings.MaxSize, overlap: strategy.Name == "fixed");
            // Todas las estrategias necesitan estos embeddings finales para retrieval.
            // Semantic ya generó vectores de unidades para cortar; aquí se calculan otros sobre los chunks finales.
            GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(generator,
                result.Chunks.Select(c => c.Content).ToArray(), token);
            dimensions ??= embeddings[0].Vector.Length;
            if (dimensions != embeddings[0].Vector.Length) throw new InvalidOperationException("La dimensión cambió dentro del experimento.");
            records.AddRange(result.Chunks.Select((c, i) => DocumentRecord.Create(c, strategy.Name, generation, embeddings[i].Vector)));
            allChunks.AddRange(result.Chunks);
            metrics.Add(new(document.Source, document.Text.Length, result.Chunks.Count, watch.Elapsed.TotalMilliseconds,
                result.AdditionalEmbeddings, result.LlmCalls, result.AdjacentSimilarities ?? []));
            logger.LogInformation("{Strategy}: {Source}, {Count} chunks, {Milliseconds:F0} ms", strategy.Name, document.Source, result.Chunks.Count, watch.Elapsed.TotalMilliseconds);
        }
        IndexManifest manifest = new(ExperimentSettings.SchemaVersion, strategy.Name, config.EmbeddingModel, config.Model,
            IndexManifest.Identity(config), dimensions!.Value, IndexManifest.HashCorpus(documents), generation,
            settings, records.Select(r => r.Id).ToArray(), allChunks.ToArray(), metrics.ToArray());
        using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(collectionName, DocumentRecord.Definition(dimensions.Value));
        // Se prepara y valida TODO antes de escribir. Qdrant y el JSON no comparten una transacción.
        await collection.EnsureCollectionExistsAsync(token);
        await collection.UpsertAsync(records, token);
        // Upsert reemplaza IDs existentes, pero no elimina posiciones desaparecidas al cambiar los cortes.
        // La diferencia contra el manifiesto anterior también elimina chunks de archivos retirados del corpus.
        Guid[] removed = previous?.Ids.Except(manifest.Ids).ToArray() ?? [];
        if (removed.Length > 0) await collection.DeleteAsync(removed, token);
        // Publicamos el manifiesto al final; una interrupción previa puede requerir reingesta para recuperar coherencia.
        await manifest.SaveAsync(artifacts, token);
        logger.LogInformation("Publicada {Collection}: {Count} puntos, dimensión {Dimensions}", collectionName, records.Count, dimensions);
        return manifest;
    }
}
