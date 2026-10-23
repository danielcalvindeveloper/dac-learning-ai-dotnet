using Xunit;

namespace RagSemanticChunking.Tests;

/// <summary>Prueba contratos compartidos: CLI, identidad, manifiestos, métricas y respuesta sin evidencia.</summary>
public sealed class ExperimentTests
{
    // La configuración se inyecta con valores ficticios: estas pruebas no leen el .env del desarrollador.
    private static AppConfiguration Config => AppConfiguration.Parse(name => name switch
    {
        "AI_PROVIDER" => "test", "AI_API_KEY" => "never-log", "AI_MODEL" => "chat",
        "AI_EMBEDDING_MODEL" => "vector", _ => null
    });

    // Snapshot mínimo válido para probar reglas del experimento, sin crear una colección real.
    internal static IndexManifest Manifest(string strategy) => new(1, strategy, "vector", "chat", IndexManifest.Identity(Config),
        2, "hash", "generation", new(), [Guid.NewGuid()], [new("a.md", 0, 0, 3, "abc", "A", "end")], [new("a.md", 3, 1, 1, 0, 0, [])]);

    [Theory]
    [InlineData("fixed")]
    [InlineData("structure")]
    [InlineData("semantic")]
    [InlineData("llm")]
    public void CliSelectsExactlyOneStrategy(string strategy)
    {
        Assert.Equal(strategy, CommandLine.Parse(["ingest", "--chunking", strategy]).Strategy);
        CommandLine query = CommandLine.Parse(["query", "pregunta", "--chunking", strategy, "--search-only"]);
        Assert.True(query.SearchOnly); Assert.Equal("pregunta", query.Question);
        Assert.Equal("dac_chunking_" + strategy, ExperimentSettings.Collection(strategy));
    }

    [Theory]
    [InlineData("ingest")]
    [InlineData("query")]
    [InlineData("query --chunking fixed")]
    [InlineData("compare --chunking fixed")]
    [InlineData("ingest --chunking unknown")]
    [InlineData("ingest --chunking fixed --chunking llm")]
    [InlineData("ingest --chunking fixed --search-only")]
    public void CliRejectsAmbiguousOrUnsupportedCommands(string input) =>
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(input.Split(' ')));

    [Fact]
    public void MetadataAndIdentityPreserveProvenanceAcrossUpserts()
    {
        DocumentChunk chunk = new("a.md", 0, 10, 13, "abc", "Guía / MFA", "LLM boundary");
        DocumentRecord first = DocumentRecord.Create(chunk, "llm", "g1", new float[] { 1, 0 });
        DocumentRecord next = DocumentRecord.Create(chunk with { Content = "def" }, "llm", "g2", new float[] { 0, 1 });
        Assert.Equal(first.Id, next.Id);
        Assert.NotEqual(first.Id, DocumentRecord.Create(chunk, "fixed", "g1", new float[] { 1, 0 }).Id);
        Assert.Equal("a.md", first.DocumentId); Assert.Equal(chunk.Section, first.Section);
        Assert.Equal(10, first.Start); Assert.Equal(13, first.End); Assert.Equal("llm", first.ChunkingStrategy);
        Assert.Equal("LLM boundary", first.BoundaryReason);
    }

    [Fact]
    public void RecallDeduplicatesDocumentsAndMrrUsesActualChunkRank()
    {
        // Dos hits de a.md no compensan que falte b.md; la relevancia esperada está a nivel documento.
        Assert.Equal(.5, Evaluation.Recall(["a.md", "b.md"], ["a.md", "a.md", "c.md"]));
        Assert.Equal(1, Evaluation.Recall(["a.md"], ["a.md", "a.md"]));
        Assert.Equal(0, Evaluation.Recall(["a.md"], []));
        Assert.Equal(.5, Evaluation.ReciprocalRank(["b.md"], ["c.md", "b.md", "b.md"]));
        Assert.Equal(0, Evaluation.ReciprocalRank(["b.md"], ["c.md"]));
        Assert.Throws<ArgumentException>(() => Evaluation.Recall([], ["a.md"]));
    }

    [Fact]
    public void ComparisonRejectsMissingStrategiesOrDifferentCorpusModelAndSettings()
    {
        IndexManifest[] manifests = CommandLine.Strategies.Select(Manifest).ToArray();
        Evaluation.ValidateComparable(manifests, Config, "hash");
        Assert.Throws<InvalidOperationException>(() => Evaluation.ValidateComparable(manifests[..3], Config, "hash"));
        Assert.Throws<InvalidOperationException>(() => Evaluation.ValidateComparable(manifests, Config, "different"));
        Assert.Throws<InvalidOperationException>(() => Evaluation.ValidateComparable(manifests, Config with { EmbeddingModel = "other" }, "hash"));
        manifests[3] = manifests[3] with { Settings = new(MaxSize: 900) };
        Assert.Throws<InvalidOperationException>(() => Evaluation.ValidateComparable(manifests, Config, "hash"));
    }

    [Fact]
    public void ReportIncludesEveryStrategyAndDoesNotDeclareAWinner()
    {
        QuerySet queries = new(1, [new("Q1", "question", ["a.md"])]);
        IndexManifest[] manifests = CommandLine.Strategies.Select(Manifest).ToArray();
        QueryMeasurement[] measurements = CommandLine.Strategies.Select(s => new QueryMeasurement("Q1", s, 1, .5, [])).ToArray();
        ComparisonReport report = Evaluation.Summarize(queries, manifests, measurements);
        Assert.Equal(4, report.Strategies.Count);
        Assert.All(report.Strategies, s => { Assert.Equal(1, s.MeanRecall); Assert.Equal(.5, s.Mrr); });
        string markdown = Evaluation.Markdown(report);
        foreach (string strategy in CommandLine.Strategies) Assert.Contains(strategy, markdown);
        Assert.Contains("Recall@3", markdown); Assert.Contains("sin ganador universal", markdown);
        Assert.Throws<ArgumentException>(() => Evaluation.Summarize(queries, manifests, measurements[..3]));
    }

    [Fact]
    public async Task ManifestRoundTripKeepsBoundariesMetricsAndSettings()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            IndexManifest original = Manifest("fixed");
            await original.SaveAsync(directory, TestContext.Current.CancellationToken);
            IndexManifest loaded = await IndexManifest.LoadAsync(directory, "fixed", TestContext.Current.CancellationToken);
            Assert.Equal(original.Settings, loaded.Settings); Assert.Equal(original.Ids, loaded.Ids);
            Assert.Equal(original.Chunks, loaded.Chunks); Assert.Equal(original.Generation, loaded.Generation);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task EmptyRetrievalSkipsChatAndContextUsesActualSources()
    {
        using RecordingChat chat = new("answer");
        RagService rag = new(chat);
        Assert.False((await rag.AnswerAsync("question", [], TestContext.Current.CancellationToken)).UsedModel);
        Assert.Equal(0, chat.Calls);
        SearchHit a = new(new() { Source = "a.md", Text = "evidence", Section = "MFA" }, .8);
        RagAnswer answer = await rag.AnswerAsync("question", [a, a], TestContext.Current.CancellationToken);
        Assert.Equal(["a.md"], answer.Sources);
        Assert.Contains("evidence", chat.Prompt); Assert.Contains("Sección: MFA", chat.Prompt);
        Assert.Equal(1, chat.Calls);
    }

    [Fact]
    public void ConfigurationHasSeparateModelsAndValidatesEndpoints()
    {
        Assert.Equal("chat", Config.Model); Assert.Equal("vector", Config.EmbeddingModel);
        Assert.Equal(new Uri("http://localhost:6334"), Config.QdrantEndpoint);
        Assert.Throws<InvalidOperationException>(() => AppConfiguration.Parse(_ => null));
        Assert.Throws<InvalidOperationException>(() => AppConfiguration.Parse(name => name == "AI_URL" ? "relative" : "value"));
    }
}
