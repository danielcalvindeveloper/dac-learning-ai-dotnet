using System.ClientModel;
using Microsoft.Extensions.AI;

namespace RagSemanticChunking;

/// <summary>Genera vectores mediante Microsoft.Extensions.AI y comprueba la forma del resultado.</summary>
public static class EmbeddingGeneration
{
    /// <summary>Vectoriza un lote conservando la correspondencia posicional entre textos y embeddings.</summary>
    /// <param name="generator">Abstracción de embeddings; la lógica no depende del SDK concreto.</param>
    /// <param name="texts">Lote no vacío, cuyo orden se usará para asociar cada vector con su texto.</param>
    /// <param name="cancellationToken">Permite cancelar la llamada remota.</param>
    /// <returns>Un embedding por texto, todos de la misma dimensión.</returns>
    /// <remarks>La dimensión la devuelve el modelo; Qdrant almacena los vectores, no los genera.</remarks>
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

        // No asumimos una dimensión fija: depende del modelo, y debe ser uniforme dentro del lote.
        int dimensions = embeddings[0].Vector.Length;
        if (embeddings.Any(embedding => embedding.Vector.Length != dimensions))
        {
            throw new InvalidOperationException("Los embeddings recibidos tienen distintas dimensiones.");
        }

        return embeddings;
    }
}
