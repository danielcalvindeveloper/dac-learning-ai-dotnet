namespace RagSemanticChunking;

/// <summary>Baseline local: limita tamaño y repite overlap, prefiriendo separadores naturales.</summary>
/// <remarks>Sirve como referencia de coste y calidad; no requiere un modelo para decidir cortes.</remarks>
public sealed class FixedChunkingStrategy(ChunkingSettings settings) : IChunkingStrategy
{
    /// <inheritdoc/>
    public string Name => "fixed";
    /// <inheritdoc/>
    public Task<ChunkingResult> ChunkAsync(DocumentText document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        settings.Validate();
        // El cálculo es síncrono. Task.FromResult respeta el contrato común sin crear un hilo adicional.
        // TextPartition intenta párrafo, oración y corte duro; Build recupera el contenido original.
        return Task.FromResult(new ChunkingResult(document.Build(TextPartition.Recursive(
            document.Text, 0, document.Text.Length, settings.MaxSize, settings.Overlap))));
    }
}
