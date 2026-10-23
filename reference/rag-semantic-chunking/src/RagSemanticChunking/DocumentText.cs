using Markdig;
using Markdig.Syntax;

namespace RagSemanticChunking;

/// <summary>Rango [Start, End) sobre un string .NET; End no pertenece al rango.</summary>
/// <remarks>Las posiciones cuentan unidades UTF-16, no tokens ni bytes del archivo.</remarks>
public sealed record TextRange(int Start, int End, string Reason = "")
{
    /// <summary>Tamaño del rango en unidades UTF-16: final excluido menos inicio incluido.</summary>
    public int Length => End - Start;
}

/// <summary>Fragmento trazable: conserva contenido, origen, posición y motivo del corte.</summary>
/// <param name="Source">Nombre del archivo de origen.</param>
/// <param name="Index">Posición del chunk dentro de ese documento, contando desde cero.</param>
/// <param name="Start">Inicio incluido en el texto normalizado.</param>
/// <param name="End">Final excluido en el texto normalizado.</param>
/// <param name="Content">Slice exacta del documento; nunca una reescritura del modelo.</param>
/// <param name="Section">Jerarquía de títulos vigente al inicio del fragmento.</param>
/// <param name="BoundaryReason">Motivo observable del corte, útil para comparar estrategias.</param>
public sealed record DocumentChunk(string Source, int Index, int Start, int End,
    string Content, string Section, string BoundaryReason);

/// <summary>Mantiene una única versión normalizada del texto y su estructura Markdown.</summary>
/// <remarks>Todas las estrategias parten de este mismo texto para que la comparación sea justa.</remarks>
public sealed class DocumentText
{
    /// <summary>Nombre de origen que acompaña a los chunks y a las fuentes recuperadas.</summary>
    public string Source { get; }
    /// <summary>Texto con saltos de línea uniformes y sin espacios exteriores.</summary>
    public string Text { get; }
    /// <summary>Árbol de bloques de Markdig; sus posiciones apuntan a <see cref="Text"/>.</summary>
    public MarkdownDocument Markdown { get; }

    /// <summary>Normaliza una vez y analiza la estructura sin convertir Markdown a HTML.</summary>
    public DocumentText(string source, string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(text);
        Source = source;
        Text = text.ReplaceLineEndings("\n").Trim();
        // Markdig reconoce bloques y posiciones. Nunca renderizamos Markdown para construir chunks.
        Markdown = Markdig.Markdown.Parse(Text);
    }

    /// <summary>Lee un archivo local .md o .txt mediante la API estándar de .NET.</summary>
    /// <remarks>Usa UTF-8 por defecto y detección de BOM; no incluye PDF ni otros parsers.</remarks>
    public static async Task<DocumentText> LoadAsync(string path, CancellationToken token)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("No existe el documento.", path);
        if (Path.GetExtension(path).ToLowerInvariant() is not (".md" or ".txt"))
            throw new NotSupportedException("Sólo se admiten .md y .txt.");
        return new(Path.GetFileName(path), await File.ReadAllTextAsync(path, token));
    }

    /// <summary>Forma unidades contiguas a partir de bloques Markdown y subdivide los que exceden maxSize.</summary>
    /// <remarks>Son unidades previas al chunk final: las estrategias semánticas pueden agrupar varias.</remarks>
    public IReadOnlyList<TextRange> Units(int maxSize)
    {
        if (Text.Length == 0) return [];
        // Un bloque CommonMark (párrafo, lista, heading, código) es una unidad observable.
        // Los separadores quedan en la unidad anterior: las slices cubren exactamente el texto.
        int[] starts = Markdown.Select(block => block.Span.Start).Prepend(0).Distinct().Order().ToArray();
        List<TextRange> units = [];
        for (int i = 0; i < starts.Length; i++)
        {
            int end = i + 1 < starts.Length ? starts[i + 1] : Text.Length;
            units.AddRange(TextPartition.Recursive(Text, starts[i], end, maxSize, 0));
        }
        return units;
    }

    /// <summary>Obtiene los títulos activos en una posición para facilitar la trazabilidad.</summary>
    /// <remarks>Describe el inicio del chunk; no afirma que todo su contenido pertenezca a una única sección.</remarks>
    public string SectionAt(int position)
    {
        List<(int Level, string Title)> headings = [];
        foreach (HeadingBlock heading in Markdown.OfType<HeadingBlock>().Where(h => h.Span.Start <= position))
        {
            // Un título del mismo nivel reemplaza al anterior; uno superior cierra sus subsecciones.
            headings.RemoveAll(item => item.Level >= heading.Level);
            // Markdig usa un final incluido; el rango de string usa final excluido, de ahí el + 1.
            string title = Text[heading.Span.Start..(heading.Span.End + 1)].Trim().TrimStart('#').Trim();
            headings.Add((heading.Level, title));
        }
        return string.Join(" / ", headings.Select(h => h.Title));
    }

    /// <summary>Materializa rangos como slices originales, agregando índice y sección a cada fragmento.</summary>
    public IReadOnlyList<DocumentChunk> Build(IReadOnlyList<TextRange> ranges) => ranges.Select((r, i) =>
        new DocumentChunk(Source, i, r.Start, r.End, Text[r.Start..r.End], SectionAt(r.Start), r.Reason)).ToArray();
}
