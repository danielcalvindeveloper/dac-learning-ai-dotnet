using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.VectorData;

namespace RagSemanticChunking;

/// <summary>Registro persistido en Qdrant: une identidad, contenido trazable y vector de búsqueda.</summary>
/// <remarks>Las propiedades mutables permiten al conector materializar registros al recuperarlos.</remarks>
public sealed class DocumentRecord
{
    /// <summary>Clave estable por estrategia, archivo e índice; permite reemplazar posiciones mediante upsert.</summary>
    public Guid Id { get; set; }
    /// <summary>Identidad del documento; en este corpus plano coincide con el nombre de archivo.</summary>
    public string DocumentId { get; set; } = "";
    /// <summary>Nombre de origen que se muestra al usuario como fuente.</summary>
    public string Source { get; set; } = "";
    /// <summary>Posición del fragmento dentro de su documento, contando desde cero.</summary>
    public int ChunkIndex { get; set; }
    /// <summary>Estrategia que produjo el fragmento; también determina su colección.</summary>
    public string ChunkingStrategy { get; set; } = "";
    /// <summary>Contenido original del chunk, usado después como contexto del modelo generativo.</summary>
    public string Text { get; set; } = "";
    /// <summary>Jerarquía de títulos activa al inicio del fragmento.</summary>
    public string Section { get; set; } = "";
    /// <summary>Inicio incluido en el documento normalizado, medido en unidades UTF-16.</summary>
    public int Start { get; set; }
    /// <summary>Final excluido; End - Start coincide con Text.Length.</summary>
    public int End { get; set; }
    /// <summary>Explicación mecánica del corte, útil para inspeccionar decisiones de la estrategia.</summary>
    public string BoundaryReason { get; set; } = "";
    /// <summary>Identifica una publicación de ingesta y permite detectar discrepancias con el manifiesto.</summary>
    public string Generation { get; set; } = "";
    /// <summary>Vector calculado con AI_EMBEDDING_MODEL; se compara con el vector de la consulta.</summary>
    public ReadOnlyMemory<float> Embedding { get; set; }

    /// <summary>Valida un chunk y lo vincula con su vector y su generación de ingesta.</summary>
    /// <remarks>
    /// El ID representa una posición, no un hash del contenido. Reingerir reemplaza esa posición;
    /// los IDs que dejan de existir se eliminan utilizando el manifiesto anterior.
    /// </remarks>
    public static DocumentRecord Create(DocumentChunk chunk, string strategy, string generation, ReadOnlyMemory<float> vector)
    {
        CommandLine.ValidateStrategy(strategy);
        if (chunk.Start < 0 || chunk.End <= chunk.Start || chunk.Content.Length != chunk.End - chunk.Start
            || chunk.Index < 0 || string.IsNullOrWhiteSpace(chunk.Source) || string.IsNullOrWhiteSpace(generation) || vector.IsEmpty)
            throw new ArgumentException("Chunk, generación o embedding inválidos.");
        // El ID identifica una posición de esta estrategia. Cambiar boundaries reemplaza esa posición.
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{strategy}\n{chunk.Source}\n{chunk.Index}"));
        return new()
        {
            Id = new(hash.AsSpan(0, 16), bigEndian: true), DocumentId = chunk.Source, Source = chunk.Source,
            ChunkIndex = chunk.Index, ChunkingStrategy = strategy, Text = chunk.Content,
            Section = chunk.Section, Start = chunk.Start, End = chunk.End,
            BoundaryReason = chunk.BoundaryReason, Generation = generation, Embedding = vector
        };
    }

    /// <summary>Describe al conector cómo mapear clave, datos y vector a la colección de Qdrant.</summary>
    /// <param name="dimensions">Dimensión real obtenida del modelo, compartida por todos los vectores de la colección.</param>
    /// <remarks>Coseno define la comparación; HNSW es el índice aproximado mantenido por Qdrant.</remarks>
    public static VectorStoreCollectionDefinition Definition(int dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);
        return new()
        {
            Properties =
            [
                // La clave identifica el punto; las propiedades de datos conservan contexto y trazabilidad.
                new VectorStoreKeyProperty(nameof(Id), typeof(Guid)),
                new VectorStoreDataProperty(nameof(DocumentId), typeof(string)),
                new VectorStoreDataProperty(nameof(Source), typeof(string)),
                new VectorStoreDataProperty(nameof(ChunkIndex), typeof(int)),
                new VectorStoreDataProperty(nameof(ChunkingStrategy), typeof(string)),
                new VectorStoreDataProperty(nameof(Text), typeof(string)),
                new VectorStoreDataProperty(nameof(Section), typeof(string)),
                new VectorStoreDataProperty(nameof(Start), typeof(int)),
                new VectorStoreDataProperty(nameof(End), typeof(int)),
                new VectorStoreDataProperty(nameof(BoundaryReason), typeof(string)),
                new VectorStoreDataProperty(nameof(Generation), typeof(string)),
                // El vector se indexa para búsqueda; su dimensión debe coincidir con la colección existente.
                new VectorStoreVectorProperty(nameof(Embedding), typeof(ReadOnlyMemory<float>), dimensions)
                { DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw }
            ]
        };
    }
}
