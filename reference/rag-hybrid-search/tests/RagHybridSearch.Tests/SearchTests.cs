using Xunit;

namespace RagHybridSearch.Tests;

public sealed class SearchTests
{
    private static DocumentRecord Record(string text, string source = "a.md", string department = "TI", int year = 2026) =>
        DocumentRecord.Create(new DocumentChunk(text, source, 0), new DocumentMetadata(department, "policy", year), new float[] { 1, 2 });

    [Fact]
    public void QdrantFilterPreservesExactAndCombinedMetadataIntent()
    {
        Func<DocumentRecord, bool> matches = new MetadataFilter("RRHH", "policy", 2026).ToExpression().Compile();
        Assert.True(matches(Record("a", department: "RRHH")));
        Assert.False(matches(Record("a", department: "rrhh")));
        Assert.False(matches(Record("a", department: "RRHH", year: 2025)));
        Assert.False(matches(Record("a", department: "TI")));
        Assert.True(new MetadataFilter().ToExpression().Compile()(Record("a")));
    }

    [Fact]
    public void RrfRewardsAgreementWithoutUsingRawScoresAndDeduplicates()
    {
        DocumentRecord a = Record("a"), b = Record("b", "b.md"), c = Record("c", "c.md");
        SearchHit[] vector = [new(a, .9), new(b, .8), new(b, .8)];
        SearchHit[] lexical = [new(c, 1000), new(b, 500)];
        IReadOnlyList<FusedHit> fused = ReciprocalRankFusion.Fuse(vector, lexical, 3);
        Assert.Equal(b.Id, fused[0].Record.Id);
        Assert.Equal(2.0 / 62, fused[0].Score, 12);
        Assert.Equal(2, fused[0].VectorRank);
        Assert.Equal(2, fused[0].Bm25Rank);
        Assert.Equal(3, fused.Select(hit => hit.Record.Id).Distinct().Count());
        Assert.Equal(fused.Select(hit => hit.Record.Id), ReciprocalRankFusion.Fuse(
            vector.Select(hit => hit with { Score = 99 }).ToArray(), lexical, 3).Select(hit => hit.Record.Id));
    }

    [Fact]
    public void RrfHandlesOneEmptyRankingStableTiesAndTopK()
    {
        DocumentRecord a = Record("a"), b = Record("b", "b.md");
        Assert.Empty(ReciprocalRankFusion.Fuse([], [], 3));
        Assert.Equal(a.Id, Assert.Single(ReciprocalRankFusion.Fuse([new(a, 1)], [], 1)).Record.Id);
        Assert.Equal(a.Id, Assert.Single(ReciprocalRankFusion.Fuse([new(b, 1)], [new(a, 4)], 1)).Record.Id);
        Assert.Throws<ArgumentOutOfRangeException>(() => ReciprocalRankFusion.Fuse([], [], 0));
    }

    [Fact]
    public void SignalAcceptanceNeverAppliesCosineThresholdToBm25OrRrf()
    {
        DocumentRecord a = Record("a"), b = Record("b", "b.md");
        SearchHit[] vector = [new(a, .1), new(b, double.NaN)];
        SearchHit[] lexical = [new(a, .01)];
        Assert.Empty(HybridSearchService.Select(SearchMode.Vector, vector, []));
        FusedHit hit = Assert.Single(HybridSearchService.Select(SearchMode.Hybrid, vector, lexical));
        Assert.Null(hit.VectorRank);
        Assert.Equal(1, hit.Bm25Rank);
    }
}
