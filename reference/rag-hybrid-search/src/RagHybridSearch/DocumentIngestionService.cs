using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;

namespace RagHybridSearch;

public sealed class DocumentIngestionService(IEmbeddingGenerator<string, Embedding<float>> generator,
    VectorStore store, ILogger<DocumentIngestionService> logger)
{
    public async Task IngestAsync(string directory, string indexPath, string model, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("No existe data/ en el output.");
        await store.CollectionExistsAsync(SearchSettings.CollectionName, cancellationToken);
        string[] paths = Directory.EnumerateFiles(directory).Where(path =>
            Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase)
            || Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal).ToArray();
        Dictionary<string, DocumentMetadata> metadata = JsonSerializer.Deserialize<Dictionary<string, DocumentMetadata>>(
            await File.ReadAllTextAsync(Path.Combine(directory, "metadata.json"), cancellationToken))
            ?? throw new InvalidOperationException("metadata.json está vacío.");
        ValidateMetadata(paths.Select(Path.GetFileName).Cast<string>().ToArray(), metadata);
        IndexMetadata? previous = null;
        Guid[] previousIds = [];
        if (LuceneIndex.Exists(indexPath))
        {
            using LexicalSearchService published = new(indexPath);
            previous = published.Metadata;
            previousIds = published.GetIds();
        }
        if (previous is not null && previous.EmbeddingModel != model)
            throw new InvalidOperationException("El índice usa otro modelo. No mezcles espacios vectoriales; prepará un índice nuevo deliberadamente.");

        List<DocumentRecord> records = [];
        string generation = Guid.NewGuid().ToString("D");
        int? dimensions = previous?.Dimensions;
        foreach (string path in paths)
        {
            string source = Path.GetFileName(path);
            IReadOnlyList<DocumentChunk> chunks = DocumentText.Split(await DocumentText.LoadAsync(path, cancellationToken),
                source, SearchSettings.ChunkSize, SearchSettings.Overlap);
            if (chunks.Count == 0) continue;
            GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(
                generator, chunks.Select(chunk => chunk.Content).ToArray(), cancellationToken);
            dimensions ??= embeddings[0].Vector.Length;
            if (embeddings[0].Vector.Length != dimensions)
                throw new InvalidOperationException("La dimensión de embeddings no coincide con el índice.");
            records.AddRange(chunks.Select((chunk, index) => DocumentRecord.Create(chunk, metadata[source], embeddings[index].Vector)));
            logger.LogInformation("Preparado {Source}: {Count} chunks", source, chunks.Count);
        }
        if (records.Count == 0) throw new InvalidOperationException("No hay texto para indexar.");
        foreach (DocumentRecord record in records) record.Generation = generation;
        using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(
            SearchSettings.CollectionName, DocumentRecord.Definition(dimensions!.Value));
        await collection.EnsureCollectionExistsAsync(cancellationToken);
        await collection.UpsertAsync(records, cancellationToken);
        // Sólo retiramos IDs conocidos por el commit anterior, en nuestra colección dedicada.
        Guid[] removed = previousIds.Except(records.Select(record => record.Id)).ToArray();
        if (removed.Length > 0) await collection.DeleteAsync(removed, cancellationToken);
        // El commit publica juntos documentos y metadata de Lucene; no es una transacción entre motores.
        LuceneIndex.Rebuild(indexPath, records, new IndexMetadata(model, dimensions.Value, generation), cancellationToken);
        logger.LogInformation("Índices publicados: {Count} chunks, dimensión {Dimensions}", records.Count, dimensions);
    }

    public static void ValidateMetadata(IReadOnlyList<string> sources, IReadOnlyDictionary<string, DocumentMetadata> metadata)
    {
        if (sources.Count == 0 || !sources.Order(StringComparer.Ordinal).SequenceEqual(metadata.Keys.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("metadata.json debe describir exactamente los documentos .md/.txt de data/.");
        if (metadata.Values.Any(value => value is null || string.IsNullOrWhiteSpace(value.Department)
            || string.IsNullOrWhiteSpace(value.DocumentType) || value.Year is < 1 or > 9999))
            throw new InvalidOperationException("Cada documento requiere Department, DocumentType y Year válidos.");
    }
}
