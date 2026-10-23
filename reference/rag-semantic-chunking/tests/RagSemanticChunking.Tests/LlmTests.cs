using Microsoft.Extensions.AI;
using Xunit;

namespace RagSemanticChunking.Tests;

/// <summary>Verifica contrato JSON, límites y rechazo de propuestas con respuestas de chat controladas.</summary>
public sealed class LlmTests
{
    private static ChunkingSettings Settings => new(MaxSize: 80, MinSize: 10, Overlap: 5, UnitMaxSize: 40);
    private static DocumentText Document => new("a.md", new string('a', 30) + "\n\n" + new string('b', 30) + "\n\n" + new string('c', 30));

    [Fact]
    public async Task StructuredProposalSelectsBoundariesAndOnlyOriginalTextBecomesChunks()
    {
        using RecordingChat chat = new("""{"EndUnit":2}""");
        ChunkingResult result = await new LlmSemanticChunkingStrategy(Settings, chat).ChunkAsync(Document, TestContext.Current.CancellationToken);
        // Además del resultado verificamos el esquema enviado: el modelo debe recibir sólo finales factibles.
        ChatResponseFormatJson format = Assert.IsType<ChatResponseFormatJson>(chat.Options!.ResponseFormat);
        Assert.Equal(2, format.Schema!.Value.GetProperty("properties").GetProperty("EndUnit").GetProperty("enum").GetArrayLength());
        Assert.Equal(1, result.LlmCalls);
        Assert.Equal(0, result.AdditionalEmbeddings);
        Assert.Equal(Document.Text, string.Concat(result.Chunks.Select(c => c.Content)));
        Assert.Equal(64, result.Chunks[0].End);
    }

    [Theory]
    [InlineData("not JSON")]
    [InlineData("```json\n{}\n```")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"EndUnit\":99}")]
    [InlineData("{\"EndUnit\":2,\"Text\":\"invented\"}")]
    public void MalformedOutputNeverBecomesSilentFallback(string json) =>
        Assert.Throws<InvalidOperationException>(() => BoundaryProposalValidator.Select(json, 0,
            BoundaryProposalValidator.FeasibleSegments(Document, Document.Units(40), Settings)));

    [Theory]
    [InlineData(0, 1, 2, 3)] // gap
    [InlineData(0, 2, 1, 3)] // overlap
    [InlineData(1, 2, 0, 3)] // order and missing beginning
    [InlineData(-1, 1, 1, 3)]
    [InlineData(0, 0, 0, 3)]
    [InlineData(0, 2, 2, 4)]
    [InlineData(0, 1, 0, 1)] // duplicate
    public void InvalidUnitRangesAreRejected(int a, int b, int c, int d)
    {
        BoundaryProposal proposal = new([new(a, b), new(c, d)]);
        Assert.Throws<InvalidOperationException>(() => BoundaryProposalValidator.Validate(Document, Document.Units(40), proposal, Settings));
    }

    [Fact]
    public void IncompleteCoverageAndSizeViolationsAreRejected()
    {
        Assert.Throws<InvalidOperationException>(() => BoundaryProposalValidator.Validate(Document, Document.Units(40), new([new(0, 2)]), Settings));
        Assert.Throws<InvalidOperationException>(() => BoundaryProposalValidator.Validate(Document, Document.Units(40), new([new(0, 3)]), Settings));
        ChunkingSettings min = new(MaxSize: 100, MinSize: 40, Overlap: 5, UnitMaxSize: 50);
        Assert.Throws<InvalidOperationException>(() => BoundaryProposalValidator.Validate(Document, Document.Units(50), new([new(0, 1), new(1, 3)]), min));
    }

    [Fact]
    public void FeasibleAlternativesRespectSizesAndAllowACompletePath()
    {
        IReadOnlyList<TextRange> units = Document.Units(40);
        IReadOnlyList<BoundarySegment> candidates = BoundaryProposalValidator.FeasibleSegments(Document, units, Settings);
        Assert.Contains(new(0, 2), candidates);
        Assert.Contains(new(2, 3), candidates);
        Assert.DoesNotContain(new(0, 3), candidates);
        Assert.All(candidates, s => Assert.InRange(units[s.EndUnit - 1].End - units[s.StartUnit].Start, 10, 80));
    }

    [Fact]
    public async Task InvalidProposalAbortsWithoutFallingBackOrRetrying()
    {
        using RecordingChat chat = new("""{"EndUnit":99}""");
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new LlmSemanticChunkingStrategy(Settings, chat).ChunkAsync(Document, TestContext.Current.CancellationToken));
        Assert.Contains("sin fallback", error.Message);
        Assert.Equal(1, chat.Calls);
    }

    [Fact]
    public async Task DocumentLimitRejectsBeforeProviderCallAndShortDocumentIsAccepted()
    {
        using RecordingChat chat = new("""{"EndUnit":1}""");
        ChunkingResult small = await new LlmSemanticChunkingStrategy(Settings, chat).ChunkAsync(new("a.md", "corto"), TestContext.Current.CancellationToken);
        Assert.Single(small.Chunks);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new LlmSemanticChunkingStrategy(Settings with { LlmMaxDocumentSize = 80 }, chat)
            .ChunkAsync(Document, TestContext.Current.CancellationToken));
        Assert.Equal(0, chat.Calls);
    }
}

/// <summary>Doble de chat que registra prompt, opciones y llamadas, y devuelve una respuesta predefinida.</summary>
/// <remarks>Permite comprobar nuestra integración sin red; no pretende probar el comportamiento de un LLM.</remarks>
internal sealed class RecordingChat(string response) : IChatClient
{
    public int Calls { get; private set; }
    public string Prompt { get; private set; } = "";
    public ChatOptions? Options { get; private set; }
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); Calls++; Options = options;
        Prompt = string.Join('\n', messages.Select(m => m.Text));
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, response)));
    }
    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
