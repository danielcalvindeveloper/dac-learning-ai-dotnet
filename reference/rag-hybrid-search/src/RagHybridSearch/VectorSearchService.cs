using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace RagHybridSearch;

public sealed class VectorSearchService(IEmbeddingGenerator<string, Embedding<float>> generator, VectorStore store,
    string collectionName = SearchSettings.CollectionName)
{
    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string question, MetadataFilter filter,
        IndexMetadata metadata, CancellationToken cancellationToken)
    {
        if (!await store.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException("No existe la colección híbrida. Ejecutá ingest.");
        GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(generator, [question], cancellationToken);
        ReadOnlyMemory<float> vector = embeddings[0].Vector;
        if (vector.Length != metadata.Dimensions) throw new InvalidOperationException("El modelo devuelve otra dimensión; revisá la ingesta.");
        using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(
            collectionName, DocumentRecord.Definition(vector.Length));
        VectorSearchOptions<DocumentRecord> options = new() { Filter = filter.ToExpression() };
        List<SearchHit> hits = [];
        // El provider traduce Filter a Qdrant: se aplica antes de seleccionar topK, no en esta lista.
        await foreach (VectorSearchResult<DocumentRecord> result in collection.SearchAsync(vector,
            SearchSettings.TopKVector, options, cancellationToken))
        {
            DocumentRecord record = result.Record;
            if (record.Generation != metadata.Generation)
                throw new InvalidOperationException("Qdrant y el commit Lucene tienen distintas generaciones. Ejecutá ingest antes de consultar.");
            if (result.Score is double score && double.IsFinite(score)) hits.Add(new SearchHit(record, score));
        }
        return hits;
    }
}
