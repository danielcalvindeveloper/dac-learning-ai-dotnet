using Xunit;

namespace RagSemanticChunking.Tests;

/// <summary>Comprueba cortes locales e integridad del texto sin modelos ni infraestructura.</summary>
/// <remarks>Los documentos pequeños y controlados permiten verificar offsets exactos y casos Unicode.</remarks>
public sealed class ChunkingTests
{
    private static ChunkingSettings Settings => new(MaxSize: 80, MinSize: 10, Overlap: 5, UnitMaxSize: 40);
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task FixedPrefersParagraphAndPreservesOverlapAndEveryCharacter()
    {
        DocumentText document = new("a.md", new string('a', 60) + "\n\n" + new string('b', 120));
        ChunkingResult result = await new FixedChunkingStrategy(Settings).ChunkAsync(document, Token);
        TextPartition.ValidateCoverage(document, result.Chunks, 80, overlap: true);
        Assert.Equal(62, result.Chunks[0].End);
        Assert.Equal("paragraph", result.Chunks[0].BoundaryReason);
        // Comprobamos tanto las posiciones como el texto repetido: un offset correcto por sí solo no demuestra overlap real.
        for (int i = 1; i < result.Chunks.Count; i++)
        {
            Assert.Equal(result.Chunks[i - 1].End - 5, result.Chunks[i].Start);
            Assert.Equal(result.Chunks[i - 1].Content[^5..], result.Chunks[i].Content[..5]);
        }
    }

    [Fact]
    public void RecursiveUsesSentenceBeforeForcedCut()
    {
        string text = new string('x', 49) + ". " + new string('y', 80);
        IReadOnlyList<TextRange> ranges = TextPartition.Recursive(text, 0, text.Length, 80, 0);
        Assert.Equal(51, ranges[0].End);
        Assert.Equal("sentence", ranges[0].Reason);
        Assert.Equal(text, string.Concat(ranges.Select(r => text[r.Start..r.End])));
    }

    [Fact]
    public void TinyUnicodeWindowReducesOverlapInsteadOfRepeatingTheSameStart()
    {
        // Cada emoji ocupa dos unidades UTF-16: esta ventana extrema detecta posibles bucles por falta de avance.
        const string text = "a😀b😀c";
        IReadOnlyList<TextRange> ranges = TextPartition.Recursive(text, 0, text.Length, 2, 1);
        Assert.InRange(ranges.Count, 1, text.Length);
        Assert.All(ranges, r => Assert.InRange(r.Length, 1, 2));
        DocumentText document = new("a.md", text);
        TextPartition.ValidateCoverage(document, document.Build(ranges), 2, overlap: true);
        Assert.All(ranges, r => Assert.False(char.IsHighSurrogate(text[r.End - 1])));
    }

    [Fact]
    public async Task ForcedCutsRespectMaximumAndUnicodeSurrogatePairs()
    {
        DocumentText document = new("a.txt", string.Concat(Enumerable.Repeat("😀", 121)));
        ChunkingResult result = await new FixedChunkingStrategy(Settings with { MaxSize = 81 }).ChunkAsync(document, Token);
        TextPartition.ValidateCoverage(document, result.Chunks, 81, overlap: true);
        Assert.All(result.Chunks, c =>
        {
            Assert.False(char.IsLowSurrogate(c.Content[0]));
            Assert.False(char.IsHighSurrogate(c.Content[^1]));
        });
    }

    [Theory]
    [InlineData(0, 10, 0, 10)]
    [InlineData(80, 0, 0, 40)]
    [InlineData(80, 10, -1, 40)]
    [InlineData(80, 10, 10, 40)]
    [InlineData(80, 10, 5, 100)]
    public void InvalidSettingsAreRejected(int max, int min, int overlap, int unit) =>
        Assert.Throws<ArgumentException>(() => new ChunkingSettings(max, min, overlap, unit).Validate());

    [Fact]
    public async Task StructurePreservesSectionWithListAndTracksNestedHeading()
    {
        DocumentText document = new("a.md", "# Guía\n\nIntro.\n\n## Acceso\n\nPasos:\n\n- Uno\n- Dos\n\n### MFA\n\nUsar llave.");
        ChunkingResult result = await new StructureAwareChunkingStrategy(Settings).ChunkAsync(document, Token);
        TextPartition.ValidateCoverage(document, result.Chunks, 80, overlap: false);
        Assert.Equal(document.Text, string.Concat(result.Chunks.Select(c => c.Content)));
        DocumentChunk list = Assert.Single(result.Chunks, c => c.Content.Contains("- Uno"));
        Assert.Contains("- Dos", list.Content);
        Assert.Equal("Guía / Acceso", list.Section);
        Assert.Equal("Guía / Acceso / MFA", result.Chunks[^1].Section);
    }

    [Fact]
    public async Task LargeSectionAndParagraphFallbackKeepCoverageAndSize()
    {
        DocumentText document = new("a.md", "## Grande\n\n" + new string('a', 150) + "\n\n- Uno\n- Dos\n\n" + new string('b', 90));
        ChunkingResult result = await new StructureAwareChunkingStrategy(Settings).ChunkAsync(document, Token);
        TextPartition.ValidateCoverage(document, result.Chunks, 80, overlap: false);
        Assert.True(result.Chunks.Count > 3);
        Assert.Contains(result.Chunks, c => c.Content.Contains("- Uno\n- Dos"));
    }

    [Fact]
    public async Task PlainTextStructureFallsBackWithoutLosingContent()
    {
        DocumentText document = new("a.txt", new string('a', 90) + "\n\n" + new string('b', 90));
        ChunkingResult result = await new StructureAwareChunkingStrategy(Settings).ChunkAsync(document, Token);
        TextPartition.ValidateCoverage(document, result.Chunks, 80, overlap: false);
    }

    [Fact]
    public async Task EmptyDocumentProducesNoChunksAndCancellationIsObserved()
    {
        DocumentText empty = new("a.md", " \r\n");
        Assert.Empty((await new FixedChunkingStrategy(Settings).ChunkAsync(empty, Token)).Chunks);
        Assert.Empty((await new StructureAwareChunkingStrategy(Settings).ChunkAsync(empty, Token)).Chunks);
        using CancellationTokenSource cancelled = new(); cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new FixedChunkingStrategy(Settings).ChunkAsync(empty, cancelled.Token));
    }

    [Fact]
    public void CoverageRejectsRewrittenTextAndGaps()
    {
        DocumentText document = new("a.md", "abcdef");
        Assert.Throws<InvalidOperationException>(() => TextPartition.ValidateCoverage(document,
            [new("a.md", 0, 0, 6, "xxxxxx", "", "")], 80, false));
        Assert.Throws<InvalidOperationException>(() => TextPartition.ValidateCoverage(document,
            [new("a.md", 0, 1, 6, "bcdef", "", "")], 80, false));
    }
}
