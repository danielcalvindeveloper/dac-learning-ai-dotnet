namespace RagHybridSearch;

public enum SearchMode { Vector, Bm25, Hybrid }

public sealed record RetrievalResult(IReadOnlyList<SearchHit> Vector, IReadOnlyList<SearchHit> Bm25,
    IReadOnlyList<FusedHit> Final);

public sealed class HybridSearchService(LexicalSearchService? lexicalSearch, VectorSearchService? vector)
{
    public async Task<RetrievalResult> SearchAsync(string question, SearchMode mode, MetadataFilter filter,
        IndexMetadata metadata, CancellationToken cancellationToken)
    {
        IReadOnlyList<SearchHit> dense = mode == SearchMode.Bm25 ? [] :
            await (vector ?? throw new InvalidOperationException("Falta el cliente vectorial."))
                .SearchAsync(question, filter, metadata, cancellationToken);
        IReadOnlyList<SearchHit> lexical = mode == SearchMode.Vector ? [] :
            (lexicalSearch ?? throw new InvalidOperationException("Falta el índice Lucene."))
                .Search(question, filter, SearchSettings.TopKBm25, cancellationToken);
        return new RetrievalResult(dense, lexical, Select(mode, dense, lexical));
    }

    public static IReadOnlyList<FusedHit> Select(SearchMode mode, IReadOnlyList<SearchHit> dense, IReadOnlyList<SearchHit> lexical)
    {
        // Aceptación por señal antes de fusionar: un score RRF no es confianza ni similitud coseno.
        SearchHit[] acceptedVector = dense.Where(hit => double.IsFinite(hit.Score)
            && hit.Score >= SearchSettings.MinimumVectorScore).ToArray();
        SearchHit[] acceptedBm25 = lexical.Where(hit => double.IsFinite(hit.Score) && hit.Score > 0).ToArray();
        return mode switch
        {
            SearchMode.Vector => acceptedVector.Take(SearchSettings.TopKFinal)
                .Select((hit, index) => new FusedHit(hit.Record, hit.Score, index + 1, null)).ToArray(),
            SearchMode.Bm25 => acceptedBm25.Take(SearchSettings.TopKFinal)
                .Select((hit, index) => new FusedHit(hit.Record, hit.Score, null, index + 1)).ToArray(),
            SearchMode.Hybrid => ReciprocalRankFusion.Fuse(acceptedVector, acceptedBm25, SearchSettings.TopKFinal),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }
}
