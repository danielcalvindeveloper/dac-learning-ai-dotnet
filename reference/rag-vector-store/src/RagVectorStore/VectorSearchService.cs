using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace RagVectorStore;

// Candidato recuperado, todavía sin aplicar el umbral de aceptación de RagService.
public sealed record SearchHit(DocumentChunkRecord Record, double Score);

public sealed class VectorSearchService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore)
{
    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string question, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        if (!await vectorStore.CollectionExistsAsync(RagSettings.CollectionName, cancellationToken))
        {
            throw new InvalidOperationException($"No existe la colección {RagSettings.CollectionName}. Ejecutá primero ingest.");
        }

        // La pregunta debe representarse en el mismo espacio vectorial que el corpus indexado.
        GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(
            embeddingGenerator, [question], cancellationToken);
        ReadOnlyMemory<float> queryVector = embeddings[0].Vector;

        using VectorStoreCollection<Guid, DocumentChunkRecord> collection = vectorStore.GetCollection<Guid, DocumentChunkRecord>(
            RagSettings.CollectionName, DocumentChunkRecord.Definition(queryVector.Length));

        List<SearchHit> hits = [];
        // El store busca y calcula los scores; topK limita candidatos, no garantiza relevancia.
        // await foreach consume resultados asíncronos; no pedimos los vectores completos de vuelta.
        await foreach (VectorSearchResult<DocumentChunkRecord> result in collection.SearchAsync(
            queryVector, top: RagSettings.TopK, cancellationToken: cancellationToken))
        {
            if (result.Score is double score && double.IsFinite(score))
            {
                hits.Add(new SearchHit(result.Record, score));
            }
        }

        return hits;
    }
}
