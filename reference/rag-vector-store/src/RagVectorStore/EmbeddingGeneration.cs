using System.ClientModel;
using Microsoft.Extensions.AI;

namespace RagVectorStore;

public static class EmbeddingGeneration
{
    public static async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEmbeddingGenerator<string, Embedding<float>> generator,
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(texts.Count);
        GeneratedEmbeddings<Embedding<float>> embeddings;
        try
        {
            // Cada resultado se emparejará por posición con el texto enviado en este lote.
            embeddings = await generator.GenerateAsync(texts, cancellationToken: cancellationToken);
        }
        catch (ClientResultException ex)
        {
            throw new InvalidOperationException(
                $"Falló la generación de embeddings (HTTP {ex.Status}). Revisá AI_EMBEDDING_MODEL, AI_API_KEY y AI_URL.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("No se pudo contactar al proveedor de embeddings. Revisá AI_URL y la conexión.", ex);
        }

        // Validamos la forma de la respuesta antes de construir records o consultar Qdrant.
        if (embeddings.Count != texts.Count || embeddings.Any(embedding => embedding.Vector.IsEmpty))
        {
            throw new InvalidOperationException("El proveedor devolvió una cantidad o dimensión de embeddings inválida.");
        }

        int dimensions = embeddings[0].Vector.Length;
        if (embeddings.Any(embedding => embedding.Vector.Length != dimensions))
        {
            throw new InvalidOperationException("Los embeddings recibidos tienen distintas dimensiones.");
        }

        return embeddings;
    }
}
