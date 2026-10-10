namespace RagHybridSearch;

public sealed record FusedHit(DocumentRecord Record, double Score, int? VectorRank, int? Bm25Rank);

public static class ReciprocalRankFusion
{
    public static IReadOnlyList<FusedHit> Fuse(IReadOnlyList<SearchHit> vector, IReadOnlyList<SearchHit> bm25,
        int topK, int constant = SearchSettings.RrfConstant)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(constant);
        Dictionary<Guid, FusedHit> fused = [];
        AddRanking(vector, true);
        AddRanking(bm25, false);
        return fused.Values.OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.Record.Source, StringComparer.Ordinal).ThenBy(hit => hit.Record.ChunkIndex)
            .Take(topK).ToArray();

        void AddRanking(IReadOnlyList<SearchHit> ranking, bool isVector)
        {
            HashSet<Guid> seen = [];
            int rank = 0;
            foreach (SearchHit hit in ranking)
            {
                // Cada ID vota una sola vez por ranking, aunque llegue repetido por error.
                if (!seen.Add(hit.Record.Id)) continue;
                rank++;
                fused.TryGetValue(hit.Record.Id, out FusedHit? previous);
                // RRF combina posiciones desde 1, no scores de escalas incompatibles.
                fused[hit.Record.Id] = new FusedHit(hit.Record,
                    (previous?.Score ?? 0) + 1.0 / (constant + rank),
                    isVector ? rank : previous?.VectorRank,
                    isVector ? previous?.Bm25Rank : rank);
            }
        }
    }
}
