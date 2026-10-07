public static class TextChunker
{
    public static IReadOnlyList<DocumentChunk> Split(
        string text,
        string source,
        int chunkSize,
        int overlap)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(chunkSize), "chunkSize debe ser mayor que cero.");
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(overlap), "overlap debe ser mayor o igual a cero y menor que chunkSize.");
        }

        string normalizedText = text.ReplaceLineEndings("\n").Trim();
        List<DocumentChunk> chunks = new();

        for (int start = 0; start < normalizedText.Length;)
        {
            int length = Math.Min(chunkSize, normalizedText.Length - start);
            chunks.Add(new DocumentChunk
            {
                Content = normalizedText.Substring(start, length),
                Source = source,
                Index = chunks.Count
            });

            // El último fragmento ya cubre el final; no repetimos su cola.
            if (length == normalizedText.Length - start)
            {
                break;
            }

            start += chunkSize - overlap;
        }

        return chunks;
    }
}
