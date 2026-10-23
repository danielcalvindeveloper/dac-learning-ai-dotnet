namespace RagSemanticChunking;

/// <summary>Resultado del corte y coste adicional atribuible a la estrategia elegida.</summary>
/// <param name="Chunks">Slices trazables del documento normalizado, en orden de lectura.</param>
/// <param name="AdditionalEmbeddings">Textos vectorizados para decidir cortes; excluye los embeddings finales comunes.</param>
/// <param name="LlmCalls">Decisiones realizadas mediante chat; excluye reintentos HTTP del SDK.</param>
/// <param name="AdjacentSimilarities">Coseno entre unidades vecinas, disponible en la estrategia semantic.</param>
public sealed record ChunkingResult(IReadOnlyList<DocumentChunk> Chunks, int AdditionalEmbeddings = 0,
    int LlmCalls = 0, IReadOnlyList<double>? AdjacentSimilarities = null);

// Cuatro implementaciones reales comparten el contrato; sólo varía la construcción de chunks.
/// <summary>Permite cambiar únicamente la construcción de chunks dentro de la misma ingesta.</summary>
/// <remarks>La interfaz tiene cuatro implementaciones reales; persistencia y recuperación permanecen comunes.</remarks>
public interface IChunkingStrategy
{
    /// <summary>Identificador estable que vincula estrategia, colección, manifiesto y reportes.</summary>
    string Name { get; }
    /// <summary>Divide el texto preservando su contenido y registra las decisiones de corte.</summary>
    /// <remarks>Las implementaciones locales completan la tarea sin red; las semánticas pueden llamar a IA.</remarks>
    Task<ChunkingResult> ChunkAsync(DocumentText document, CancellationToken cancellationToken);
}
