using Xunit;

namespace RagVectorStore.Tests;

public sealed class AppConfigurationTests
{
    private static Dictionary<string, string?> Valid() => new()
    {
        ["AI_PROVIDER"] = "openai",
        ["AI_API_KEY"] = "test-key",
        ["AI_MODEL"] = "chat-test",
        ["AI_EMBEDDING_MODEL"] = "embedding-test"
    };

    [Fact]
    public void KeepsModelsSeparateAndDocumentsLocalQdrantDefault()
    {
        Dictionary<string, string?> values = Valid();
        AppConfiguration config = AppConfiguration.Parse(name => values.GetValueOrDefault(name));
        Assert.Equal("chat-test", config.Model);
        Assert.Equal("embedding-test", config.EmbeddingModel);
        Assert.Null(config.Endpoint);
        Assert.Equal(new Uri("http://localhost:6334"), config.QdrantEndpoint);
    }

    [Theory]
    [InlineData("AI_PROVIDER")]
    [InlineData("AI_API_KEY")]
    [InlineData("AI_MODEL")]
    [InlineData("AI_EMBEDDING_MODEL")]
    public void RequiredVariablesHaveNoHiddenDefaults(string name)
    {
        Dictionary<string, string?> values = Valid();
        values[name] = " ";
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            AppConfiguration.Parse(key => values.GetValueOrDefault(key)));
        Assert.Contains(name, error.Message);
    }

    [Theory]
    [InlineData("AI_URL", "relative/path")]
    [InlineData("AI_URL", "ftp://localhost")]
    [InlineData("QDRANT_ENDPOINT", "bad")]
    [InlineData("QDRANT_ENDPOINT", "http://localhost:6334/path")]
    [InlineData("QDRANT_ENDPOINT", "http://localhost:6334?key=secret")]
    public void RejectsInvalidEndpoints(string name, string value)
    {
        Dictionary<string, string?> values = Valid();
        values[name] = value;
        Assert.Throws<InvalidOperationException>(() => AppConfiguration.Parse(key => values.GetValueOrDefault(key)));
    }

    [Fact]
    public void AllowsExplicitOpenAICompatibleEndpoint()
    {
        Dictionary<string, string?> values = Valid();
        values["AI_URL"] = "https://example.com/v1/";
        values["QDRANT_ENDPOINT"] = "http://localhost:7334";
        AppConfiguration config = AppConfiguration.Parse(key => values.GetValueOrDefault(key));
        Assert.Equal(new Uri(values["AI_URL"]!), config.Endpoint);
        Assert.Equal(7334, config.QdrantEndpoint.Port);
    }
}
