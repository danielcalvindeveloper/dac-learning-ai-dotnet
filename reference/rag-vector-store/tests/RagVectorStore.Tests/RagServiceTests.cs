using Microsoft.Extensions.AI;
using Xunit;

namespace RagVectorStore.Tests;

public sealed class RagServiceTests
{
    private static SearchHit Hit(string source, int index, double score) => new(
        new DocumentChunkRecord { Source = source, DocumentId = source, ChunkIndex = index, Text = $"Evidence {index}" }, score);

    [Fact]
    public void ContextKeepsChunkProvenanceAndDeduplicatesSources()
    {
        RagContext context = RagContext.Build([Hit("a.md", 2, .9), Hit("a.md", 3, .8), Hit("b.md", 0, .7)]);
        Assert.Equal(["a.md", "b.md"], context.Sources);
        Assert.Contains("Fuente: a.md | Documento: a.md | Chunk: 2", context.Text);
        Assert.Contains("Evidence 3", context.Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EmptyOrInsufficientRetrievalDoesNotCallModel(bool hasCandidate)
    {
        using RecordingChatClient chat = new();
        RagService rag = new(chat);
        SearchHit[] candidates = hasCandidate ? [Hit("a.md", 0, RagSettings.MinimumScore - .01)] : [];
        RagAnswer answer = await rag.AnswerAsync("question", candidates, TestContext.Current.CancellationToken);
        Assert.False(answer.UsedModel);
        Assert.Empty(answer.Sources);
        Assert.Equal(0, chat.Calls);
    }

    [Fact]
    public async Task SendsOnlyAcceptedEvidenceAndReportsSourcesFromRetrieval()
    {
        using RecordingChatClient chat = new();
        RagService rag = new(chat);
        RagAnswer answer = await rag.AnswerAsync("My question", [
            Hit("accepted.md", 1, RagSettings.MinimumScore),
            Hit("rejected.md", 2, .1),
            Hit("invalid.md", 3, double.NaN)
        ], TestContext.Current.CancellationToken);

        Assert.True(answer.UsedModel);
        Assert.Equal(1, chat.Calls);
        Assert.Equal(["accepted.md"], answer.Sources);
        Assert.Contains("My question", chat.LastPrompt);
        Assert.Contains("accepted.md", chat.LastPrompt);
        Assert.DoesNotContain("rejected.md", chat.LastPrompt);
        Assert.Contains("evidencia suficiente", chat.LastPrompt);
    }

    // Registra solicitudes sin red: comprobamos qué enviamos y si llamamos, no el razonamiento de un LLM.
    private sealed class RecordingChatClient : IChatClient
    {
        public int Calls { get; private set; }
        public string LastPrompt { get; private set; } = string.Empty;

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            LastPrompt = string.Join("\n", messages.Select(message => message.Text));
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Test response")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
