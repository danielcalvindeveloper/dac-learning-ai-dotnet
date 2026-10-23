using Markdig.Syntax;

namespace RagSemanticChunking;

/// <summary>Usa títulos y bloques Markdown para conservar estructura sin consultar modelos.</summary>
/// <remarks>Una sección corta puede quedar por debajo de MinSize; no se fusiona a costa de cruzar títulos.</remarks>
public sealed class StructureAwareChunkingStrategy(ChunkingSettings settings) : IChunkingStrategy
{
    /// <inheritdoc/>
    public string Name => "structure";
    /// <inheritdoc/>
    public Task<ChunkingResult> ChunkAsync(DocumentText document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        settings.Validate();
        List<TextRange> ranges = [];
        // El parser distingue títulos reales de texto dentro de bloques de código.
        // Los extremos 0 y Length incluyen también la introducción anterior al primer título.
        int[] boundaries = document.Markdown.OfType<HeadingBlock>().Select(h => h.Span.Start)
            .Prepend(0).Append(document.Text.Length).Distinct().Order().ToArray();
        for (int i = 0; i < boundaries.Length - 1; i++)
        {
            int start = boundaries[i], end = boundaries[i + 1];
            if (end - start <= settings.MaxSize) ranges.Add(new(start, end, "heading/section"));
            else
            {
                // Conservamos listas y párrafos enteros mientras entren; sólo el bloque grande cae al baseline.
                TextRange[] blocks = document.Units(settings.MaxSize).Where(u => u.Start >= start && u.End <= end).ToArray();
                // Agrupamos bloques consecutivos mientras entren. Si el próximo excede el máximo,
                // cerramos antes de ese bloque y comenzamos un nuevo chunk; no se agrega overlap.
                int current = start;
                foreach (TextRange block in blocks)
                {
                    if (block.End - current > settings.MaxSize)
                    { ranges.Add(new(current, block.Start, "block boundary")); current = block.Start; }
                }
                if (current < end) ranges.Add(new(current, end, "section end"));
            }
        }
        // Units ya subdividió los bloques demasiado grandes usando el corte local compartido.
        return Task.FromResult(new ChunkingResult(document.Build(ranges)));
    }
}
