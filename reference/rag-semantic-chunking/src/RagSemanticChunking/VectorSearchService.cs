using Microsoft.Extensions.VectorData;

namespace RagSemanticChunking;

/// <summary>Un chunk recuperado y su score de similitud; el score no es una probabilidad de respuesta correcta.</summary>
public sealed record SearchHit(DocumentRecord Record, double Score);

/// <summary>Consulta una colección ya ingerida y conserva los registros necesarios para mostrar fuentes.</summary>
public sealed class VectorSearchService(VectorStore store, string collectionPrefix = ExperimentSettings.CollectionPrefix)
{
    /// <summary>Recupera los TopK chunks más cercanos a un vector de consulta ya calculado.</summary>
    /// <param name="vector">Embedding de la pregunta; compare reutiliza exactamente el mismo en las cuatro colecciones.</param>
    /// <param name="manifest">Snapshot que identifica estrategia, dimensión y generación esperadas.</param>
    /// <param name="token">Permite cancelar la consulta remota.</param>
    /// <remarks>No reingiere documentos ni aplica threshold; TopK cuenta chunks, no documentos distintos.</remarks>
    public async Task<IReadOnlyList<SearchHit>> SearchAsync(ReadOnlyMemory<float> vector, IndexManifest manifest, CancellationToken token)
    {
        string name = ExperimentSettings.Collection(manifest.Strategy, collectionPrefix);
        if (!await store.CollectionExistsAsync(name, token)) throw new InvalidOperationException($"Falta {name}. Ejecutá ingest.");
        if (vector.Length != manifest.Dimensions) throw new InvalidOperationException("Dimensión incompatible con la ingesta.");
        using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(name, DocumentRecord.Definition(vector.Length));
        List<SearchHit> hits = [];
        await foreach (VectorSearchResult<DocumentRecord> result in collection.SearchAsync(vector, ExperimentSettings.TopK, cancellationToken: token))
        {
            // Validamos cada hit contra el snapshot local para detectar una publicación incompleta.
            // Esto comprueba los resultados recibidos, no audita todos los puntos de la colección.
            if (result.Record.Generation != manifest.Generation || result.Record.ChunkingStrategy != manifest.Strategy)
                throw new InvalidOperationException("Colección y manifiesto no coinciden. Reingestá antes de consultar.");
            if (result.Score is double score && double.IsFinite(score)) hits.Add(new(result.Record, score));
        }
        return hits;
    }
}
