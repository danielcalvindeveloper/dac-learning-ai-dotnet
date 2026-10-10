using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace RagHybridSearch.Tests;

public sealed class RagServiceTests
{
    [Fact]
    public async Task EmptyRetrievalNeverCallsChat()
    {
        using RecordingChatClient chat = new();
        RagAnswer answer = await new RagService(chat, NullLogger<RagService>.Instance).AnswerAsync("q", [], TestContext.Current.CancellationToken);
        Assert.False(answer.UsedModel);
        Assert.Equal(0, chat.Calls);
        Assert.Empty(answer.Sources);
    }

    [Fact]
    public async Task ContextIncludesActualRankedChunksAndSourcesAreDeduplicated()
    {
        using RecordingChatClient chat = new();
        DocumentRecord a = new() { Source = "a.md", Text = "Evidence A", Department = "RRHH", Year = 2026 };
        DocumentRecord b = new() { Source = "a.md", Text = "Evidence B", ChunkIndex = 1 };
        RagAnswer answer = await new RagService(chat, NullLogger<RagService>.Instance).AnswerAsync("my question",
            [new(a, .03, 1, 2), new(b, .02, 2, null)], TestContext.Current.CancellationToken);
        Assert.True(answer.UsedModel);
        Assert.Equal(["a.md"], answer.Sources);
        Assert.Contains("[1] Fuente: a.md | Chunk: 0", chat.Prompt);
        Assert.Contains("Evidence B", chat.Prompt);
        Assert.Contains("my question", chat.Prompt);
    }

    private sealed class RecordingChatClient : IChatClient
    {
        public int Calls { get; private set; }
        public string Prompt { get; private set; } = "";
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            Prompt = string.Join('\n', messages.Select(message => message.Text));
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Controlled response")));
        }
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
