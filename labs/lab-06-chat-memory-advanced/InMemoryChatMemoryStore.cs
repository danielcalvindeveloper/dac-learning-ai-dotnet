using Microsoft.Extensions.AI;

public sealed class InMemoryChatMemoryStore : IChatMemoryStore
{
    private const int MaxMessages = 20;

    private readonly Dictionary<string, List<ChatMessage>> _sessions = [];

    public IReadOnlyList<ChatMessage> GetMessages(
        string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out List<ChatMessage>? messages))
        {
            return [];
        }

        return messages.ToList();
    }

    public void AddMessage(
        string sessionId,
        ChatMessage message)
    {
        List<ChatMessage> messages = GetOrCreateSession(sessionId);

        messages.Add(message);

        TrimWindow(messages);
    }

    public void AddMessages(
        string sessionId,
        IEnumerable<ChatMessage> messages)
    {
        List<ChatMessage> sessionMessages =
            GetOrCreateSession(sessionId);

        sessionMessages.AddRange(messages);

        TrimWindow(sessionMessages);
    }

    private List<ChatMessage> GetOrCreateSession(
        string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out List<ChatMessage>? messages))
        {
            messages = [];
            _sessions[sessionId] = messages;
        }

        return messages;
    }

    private static void TrimWindow(
        List<ChatMessage> messages)
    {
        if (messages.Count <= MaxMessages)
        {
            return;
        }

        int messagesToRemove =
            messages.Count - MaxMessages;

        messages.RemoveRange(
            0,
            messagesToRemove);
    }
}
