using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.VectorData;

namespace RagHybridSearch;

public sealed record DocumentMetadata(string Department, string DocumentType, int Year);

public sealed class DocumentRecord
{
    public Guid Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public int Year { get; set; }
    // Detecta resultados de Qdrant que no pertenecen al commit lexical publicado.
    public string Generation { get; set; } = string.Empty;
    public ReadOnlyMemory<float> Embedding { get; set; }

    public static DocumentRecord Create(DocumentChunk chunk, DocumentMetadata metadata, ReadOnlyMemory<float> vector)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(chunk.Source);
        ArgumentException.ThrowIfNullOrWhiteSpace(chunk.Content);
        ArgumentOutOfRangeException.ThrowIfNegative(chunk.Index);
        if (vector.IsEmpty) throw new ArgumentException("El vector no puede estar vacío.", nameof(vector));
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(
            chunk.Source + "\n" + chunk.Index.ToString(CultureInfo.InvariantCulture)));
        return new DocumentRecord
        {
            Id = new Guid(hash.AsSpan(0, 16), bigEndian: true),
            Source = chunk.Source, ChunkIndex = chunk.Index, Text = chunk.Content,
            Department = metadata.Department, DocumentType = metadata.DocumentType, Year = metadata.Year,
            Embedding = vector
        };
    }

    public static VectorStoreCollectionDefinition Definition(int dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);
        return new()
        {
            Properties =
            [
                new VectorStoreKeyProperty(nameof(Id), typeof(Guid)),
                new VectorStoreDataProperty(nameof(Source), typeof(string)),
                new VectorStoreDataProperty(nameof(ChunkIndex), typeof(int)),
                new VectorStoreDataProperty(nameof(Text), typeof(string)),
                // El provider crea índices de payload para los campos que filtramos en Qdrant.
                new VectorStoreDataProperty(nameof(Department), typeof(string)) { IsIndexed = true },
                new VectorStoreDataProperty(nameof(DocumentType), typeof(string)) { IsIndexed = true },
                new VectorStoreDataProperty(nameof(Year), typeof(int)) { IsIndexed = true },
                new VectorStoreDataProperty(nameof(Generation), typeof(string)),
                new VectorStoreVectorProperty(nameof(Embedding), typeof(ReadOnlyMemory<float>), dimensions)
                {
                    DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw
                }
            ]
        };
    }
}
