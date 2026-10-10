using Xunit;

namespace RagVectorStore.Tests;

public sealed class DocumentTextTests
{
    [Fact]
    public void SplitPreservesOverlapAndFinalChunk()
    {
        IReadOnlyList<DocumentChunk> chunks = DocumentText.Split("abcdefghijkl", "guide.md", 5, 2);

        Assert.Equal(["abcde", "defgh", "ghijk", "jkl"], chunks.Select(chunk => chunk.Content));
        Assert.Equal([0, 1, 2, 3], chunks.Select(chunk => chunk.Index));
        Assert.All(chunks, chunk => Assert.Equal("guide.md", chunk.Source));
        for (int i = 1; i < chunks.Count; i++)
        {
            Assert.Equal(chunks[i - 1].Content[^2..], chunks[i].Content[..2]);
        }
    }

    [Fact]
    public void SplitStopsWhenLastChunkReachesEnd()
    {
        Assert.Equal(["abcde", "defgh"], DocumentText.Split("abcdefgh", "a.md", 5, 2).Select(chunk => chunk.Content));
    }

    [Fact]
    public void SplitNormalizesLineEndingsAndTrimsOnlyDocumentEdges()
    {
        DocumentChunk chunk = Assert.Single(DocumentText.Split("  A\r\nB\rC  ", "a.md", 20, 0));
        Assert.Equal("A\nB\nC", chunk.Content);
        Assert.Empty(DocumentText.Split(" \r\n ", "a.md", 20, 0));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(5, -1)]
    [InlineData(5, 5)]
    [InlineData(5, 6)]
    public void SplitRejectsInvalidWindow(int size, int overlap)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentText.Split("abc", "a.md", size, overlap));
    }

    [Fact]
    public async Task LoadReportsMissingFile()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".md");
        await Assert.ThrowsAsync<FileNotFoundException>(() => DocumentText.LoadAsync(path, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(".md")]
    [InlineData(".txt")]
    [InlineData(".MD")]
    [InlineData(".pdf")]
    public async Task LoadAcceptsOnlySupportedFormats(string extension)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        try
        {
            await File.WriteAllTextAsync(path, "áéí\ntexto", TestContext.Current.CancellationToken);
            if (extension == ".pdf")
            {
                await Assert.ThrowsAsync<NotSupportedException>(() => DocumentText.LoadAsync(path, TestContext.Current.CancellationToken));
            }
            else
            {
                Assert.Equal("áéí\ntexto", await DocumentText.LoadAsync(path, TestContext.Current.CancellationToken));
            }
        }
        finally
        {
            File.Delete(path);
        }
    }
}
