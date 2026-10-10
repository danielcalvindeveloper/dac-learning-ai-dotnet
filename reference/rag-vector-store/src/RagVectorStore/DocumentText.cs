namespace RagVectorStore;

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

        string normalized = text.ReplaceLineEndings("\n").Trim();
        List<DocumentChunk> chunks = [];
        for (int start = 0; start < normalized.Length; start += chunkSize - overlap)
        {
            int length = Math.Min(chunkSize, normalized.Length - start);
            chunks.Add(new DocumentChunk(normalized.Substring(start, length), source, chunks.Count));
            if (start + length == normalized.Length)
            {
                break;
            }
        }

        return chunks;
    }
}
