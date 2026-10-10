using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Xunit;

namespace RagHybridSearch.Tests;

// Integración local con Lucene: discos temporales, sin Docker, red ni modelos.
// Comprobamos nuestra configuración y mapeo, no la fórmula interna de BM25.
public sealed class LuceneIndexTests : IDisposable
{
    private readonly string path = Path.Combine(Path.GetTempPath(), "hybrid-lucene-" + Guid.NewGuid().ToString("N"));
    private readonly IndexMetadata metadata = new("test-embedding", 2, Guid.NewGuid().ToString("D"));
    private CancellationToken Token => TestContext.Current.CancellationToken;

    private DocumentRecord Record(string text, string source, string department = "TI", string type = "policy", int year = 2026, int index = 0)
    {
        DocumentRecord record = DocumentRecord.Create(new(text, source, index), new(department, type, year), new float[] { 1, 0 });
        record.Generation = metadata.Generation;
        return record;
    }

    [Fact]
    public void AnalyzerPreservesIdentifiersAndNormalizesCasingWithoutDroppingAccents()
    {
        using CodePreservingAnalyzer analyzer = new();
        using TokenStream stream = analyzer.GetTokenStream(LuceneIndex.Text, new StringReader("ART-93823, MFA: Licencia; año 2026."));
        ICharTermAttribute term = stream.AddAttribute<ICharTermAttribute>();
        List<string> terms = [];
        stream.Reset();
        while (stream.IncrementToken()) terms.Add(term.ToString());
        stream.End();
        Assert.Equal(["art-93823", "mfa", "licencia", "año", "2026"], terms);
    }

    [Fact]
    public void CommitSurvivesReopeningAndStoredFieldsMapBackToSharedRecord()
    {
        DocumentRecord expected = Record("ART-93823 contacto MFA", "catalog.md", "RRHH");
        LuceneIndex.Rebuild(path, [expected], metadata, Token);
        using (LexicalSearchService first = new(path)) Assert.Equal(1, first.Count);
        using LexicalSearchService reopened = new(path);
        SearchHit hit = Assert.Single(reopened.Search("art-93823", new(), 3, Token));
        Assert.Equal(metadata, reopened.Metadata);
        Assert.Equal(expected.Id, hit.Record.Id);
        Assert.Equal(expected.Text, hit.Record.Text);
        Assert.Equal("RRHH", hit.Record.Department);
        Assert.Equal("policy", hit.Record.DocumentType);
        Assert.Equal(2026, hit.Record.Year);
        Assert.Equal(0, hit.Record.ChunkIndex);
        Assert.True(hit.Record.Embedding.IsEmpty);
        Assert.True(double.IsFinite(hit.Score) && hit.Score > 0);
        Assert.Empty(reopened.Search("93823", new(), 3, Token));
    }

    [Fact]
    public void ReingestionReplacesContentKeepsIdentityAndRemovesRetiredChunks()
    {
        DocumentRecord first = Record("oldword", "a.md"), retired = Record("retired", "b.md");
        LuceneIndex.Rebuild(path, [first, retired], metadata, Token);
        DocumentRecord updated = Record("newword", "a.md");
        LuceneIndex.Rebuild(path, [updated], metadata, Token);
        LuceneIndex.Rebuild(path, [updated], metadata, Token);
        using LexicalSearchService search = new(path);
        Assert.Equal(first.Id, updated.Id);
        Assert.Equal(1, search.Count);
        Assert.Equal([updated.Id], search.GetIds());
        Assert.Empty(search.Search("oldword retired", new(), 3, Token));
        Assert.Equal("newword", Assert.Single(search.Search("newword", new(), 3, Token)).Record.Text);
    }

    [Fact]
    public void NativeMetadataFilterPrecedesTopKWithoutChangingLexicalScores()
    {
        DocumentRecord excluded = Record("alpha alpha alpha", "a.md");
        DocumentRecord accepted = Record("alpha", "b.md", "RRHH");
        LuceneIndex.Rebuild(path, [excluded, accepted, Record("alpha", "c.md", "RRHH", year: 2025),
            Record("alpha", "d.md", "RRHH", "guide")], metadata, Token);
        using LexicalSearchService search = new(path);
        SearchHit hit = Assert.Single(search.Search("alpha", new("RRHH", "policy", 2026), 1, Token));
        Assert.Equal(accepted.Id, hit.Record.Id);
        Assert.Equal(search.Search("alpha", new(), 10, Token).Single(h => h.Record.Id == accepted.Id).Score, hit.Score);
        Assert.Empty(search.Search("alpha", new("rrhh"), 1, Token));
    }

    [Fact]
    public void QueryUsesLiteralAnalyzedTermsHasStableTiesAndRejectsExcessiveTerms()
    {
        LuceneIndex.Rebuild(path, [Record("MFA ART-93823", "z.md"), Record("MFA ART-93823", "a.md", index: 1),
            Record("MFA ART-93823", "a.md")], metadata, Token);
        using LexicalSearchService search = new(path);
        SearchHit[] hits = search.Search("mfa", new(), 2, Token).ToArray();
        Assert.Equal(["a.md", "a.md"], hits.Select(hit => hit.Record.Source));
        Assert.Equal([0, 1], hits.Select(hit => hit.Record.ChunkIndex));
        Assert.Equal(3, search.Search("ART-93823 OR absent", new(), 5, Token).Count);
        Assert.Empty(search.Search("...", new(), 1, Token));
        Assert.Empty(search.Search("missing", new(), 1, Token));
        Assert.Throws<ArgumentOutOfRangeException>(() => search.Search("MFA", new(), 0, Token));
        string excessive = string.Join(' ', Enumerable.Range(0, SearchSettings.MaximumQueryTerms + 1).Select(i => "word" + i));
        Assert.Throws<ArgumentException>(() => search.Search(excessive, new(), 1, Token));
    }

    [Fact]
    public void CancelledRebuildKeepsLastCommitAndMissingIndexIsAnActionableError()
    {
        Assert.False(LuceneIndex.Exists(path));
        Assert.Throws<InvalidOperationException>(() => new LexicalSearchService(path));
        LuceneIndex.Rebuild(path, [Record("original", "a.md")], metadata, Token);
        using CancellationTokenSource cancelled = CancellationTokenSource.CreateLinkedTokenSource(Token);
        cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => LuceneIndex.Rebuild(path, [Record("changed", "a.md")], metadata, cancelled.Token));
        using LexicalSearchService search = new(path);
        Assert.Single(search.Search("original", new(), 3, Token));
        Assert.Throws<OperationCanceledException>(() => search.Search("original", new(), 3, cancelled.Token));
    }

    public void Dispose() { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); }
}
