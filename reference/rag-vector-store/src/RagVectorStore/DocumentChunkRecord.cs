using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.VectorData;

namespace RagVectorStore;

public sealed class DocumentChunkRecord
{
    public Guid Id { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public ReadOnlyMemory<float> Embedding { get; set; }

    public static DocumentChunkRecord FromChunk(string documentId, DocumentChunk chunk, ReadOnlyMemory<float> vector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentOutOfRangeException.ThrowIfNegative(chunk.Index);
        if (vector.IsEmpty)
        {
            throw new ArgumentException("El embedding no puede estar vacío.", nameof(vector));
        }

        // No incluye el contenido: una nueva ingesta reemplaza el chunk en la misma posición.
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(
            documentId + "\n" + chunk.Index.ToString(CultureInfo.InvariantCulture)));

        return new DocumentChunkRecord
        {
            // Usamos 16 bytes del hash y un orden explícito para obtener siempre el mismo Guid.
            Id = new Guid(hash.AsSpan(0, 16), bigEndian: true),
            DocumentId = documentId,
            Source = chunk.Source,
            ChunkIndex = chunk.Index,
            Text = chunk.Content,
            Embedding = vector
        };
    }

    public static VectorStoreCollectionDefinition Definition(int dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);

        // Definición programática para tomar la dimensión del modelo activo, sin fijarla en un atributo.
        return new VectorStoreCollectionDefinition
        {
            Properties =
            [
                // Key identifica el punto; Data conserva texto y procedencia; Vector permite buscarlo.
                new VectorStoreKeyProperty(nameof(Id), typeof(Guid)),
                new VectorStoreDataProperty(nameof(DocumentId), typeof(string)),
                new VectorStoreDataProperty(nameof(Source), typeof(string)),
                new VectorStoreDataProperty(nameof(ChunkIndex), typeof(int)),
                new VectorStoreDataProperty(nameof(Text), typeof(string)),
                new VectorStoreVectorProperty(nameof(Embedding), typeof(ReadOnlyMemory<float>), dimensions)
                {
                    DistanceFunction = DistanceFunction.CosineSimilarity,
                    IndexKind = IndexKind.Hnsw
                }
            ]
        };
    }
}
