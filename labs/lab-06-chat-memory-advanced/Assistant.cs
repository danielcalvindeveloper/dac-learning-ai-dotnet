using Microsoft.Extensions.AI;

public sealed class Assistant : IAssistant
{
    private readonly IChatClient _chatClient;
    private readonly IChatMemoryStore _memoryStore;

    public Assistant(
        IChatClient chatClient,
        IChatMemoryStore memoryStore)
    {
        _chatClient = chatClient;
        _memoryStore = memoryStore;
    }

    public async Task<string> ChatAsync(
        string sessionId,
        string message)
    {
        _memoryStore.AddMessage(
            sessionId,
            new ChatMessage(
                ChatRole.User,
                message));

        IReadOnlyList<ChatMessage> history =
            _memoryStore.GetMessages(sessionId);

        ChatResponse response =
            await _chatClient.GetResponseAsync(history);

        _memoryStore.AddMessages(
            sessionId,
            response.Messages);

        return response.Text;
    }
}
