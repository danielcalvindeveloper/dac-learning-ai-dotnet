using Xunit;

namespace RagHybridSearch.Tests;

public sealed class ConfigurationAndIndexTests
{
    [Theory]
    [InlineData("--mode", "invalid")]
    [InlineData("--year", "0")]
    [InlineData("--year", "wrong")]
    [InlineData("--unknown", "a")]
    public void RejectsInvalidOptions(string option, string value) =>
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["query", "question", option, value]));

    [Fact]
    public void ParsesModeFilterAndSearchOnly()
    {
        CommandLine command = CommandLine.Parse(["query", "annual leave", "--mode", "bm25", "--department", "RRHH", "--type", "policy", "--year", "2026", "--search-only"]);
        Assert.Equal(SearchMode.Bm25, command.Mode);
        Assert.True(command.SearchOnly);
        Assert.Equal(new MetadataFilter("RRHH", "policy", 2026), command.Filter);
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["query", "q", "--year"]));
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["query", "q", "--mode", "bm25", "--mode", "vector"]));
    }

    [Fact]
    public void ValidatesManifestWithoutHiddenMetadataDefaults()
    {
        Dictionary<string, DocumentMetadata> metadata = new() { ["a.md"] = new("RRHH", "policy", 2026) };
        DocumentIngestionService.ValidateMetadata(["a.md"], metadata);
        Assert.Throws<InvalidOperationException>(() => DocumentIngestionService.ValidateMetadata(["b.md"], metadata));
        metadata["a.md"] = new("", "policy", 2026);
        Assert.Throws<InvalidOperationException>(() => DocumentIngestionService.ValidateMetadata(["a.md"], metadata));
    }

    [Fact]
    public void IdentitySeparatesSourcesAndPositionsAndSchemaRequiresDimensions()
    {
        DocumentMetadata metadata = new("TI", "guide", 2026);
        DocumentRecord first = DocumentRecord.Create(new("same text", "a.md", 0), metadata, new float[] { 1, 2 });
        DocumentRecord anotherSource = DocumentRecord.Create(new("same text", "b.md", 0), metadata, new float[] { 1, 2 });
        DocumentRecord anotherPosition = DocumentRecord.Create(new("same text", "a.md", 1), metadata, new float[] { 1, 2 });
        Assert.Equal(3, new[] { first.Id, anotherSource.Id, anotherPosition.Id }.Distinct().Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentRecord.Definition(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DocumentRecord.Definition(-1));
    }

    [Fact]
    public void CommitMetadataValidatesSchemaModelDimensionAndGeneration()
    {
        IndexMetadata expected = new("embedding-test", 2, Guid.NewGuid().ToString("D"));
        Assert.Equal(expected, IndexMetadata.FromCommitData(expected.ToCommitData()));
        foreach (string field in new[] { "schema", "model", "dimensions", "generation" })
        {
            Dictionary<string, string> invalid = expected.ToCommitData();
            invalid[field] = "";
            Assert.Throws<InvalidOperationException>(() => IndexMetadata.FromCommitData(invalid));
        }
    }
}
