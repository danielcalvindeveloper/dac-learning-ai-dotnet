namespace RagHybridSearch;

// Fragmento previo a la indexación: conserva procedencia y posición, pero todavía no un vector.
public sealed record DocumentChunk(string Content, string Source, int Index);

public static class DocumentText
{
    public static async Task<string> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("No se encontró el documento local.", path);
        }

        string extension = Path.GetExtension(path);
        if (!extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException("Sólo se admiten documentos .md y .txt.");
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public static IReadOnlyList<DocumentChunk> Split(string text, string source, int chunkSize, int overlap)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        ArgumentOutOfRangeException.ThrowIfNegative(overlap);
        if (overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap), "Debe ser menor que chunkSize.");
        }

        // Recortamos sólo el documento completo para no alterar el overlap entre fragmentos.
        string normalized = text.ReplaceLineEndings("\n").Trim();
        List<DocumentChunk> chunks = [];
        // El inicio avanza menos que el tamaño: así se repite la cola del chunk anterior.
        // Length y Substring cuentan unidades UTF-16, no tokens ni límites de palabras.
        for (int start = 0; start < normalized.Length; start += chunkSize - overlap)
        {
            int length = Math.Min(chunkSize, normalized.Length - start);
            chunks.Add(new DocumentChunk(normalized.Substring(start, length), source, chunks.Count));
            if (start + length == normalized.Length)
            {
                // No agregamos otro fragmento formado sólo por texto que ya alcanzó el final.
                break;
            }
        }

        return chunks;
    }
}
