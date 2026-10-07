public sealed class InMemoryVectorStore
{
    private readonly List<IndexedChunk> _chunks = [];

    public void Add(IndexedChunk chunk)
    {
        _chunks.Add(chunk);
    }

    public IReadOnlyList<SearchResult> Search(
        ReadOnlyMemory<float> queryVector,
        int topK)
    {
        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topK), "topK debe ser mayor que cero.");
        }

        return _chunks
            .Select(indexed => new SearchResult(
                indexed.Chunk,
                VectorSimilarity.CosineSimilarity(
                    queryVector.Span,
                    indexed.Vector.Span)))
            .OrderByDescending(result => result.Similarity)
            .Take(topK)
            .ToArray();
    }
}
