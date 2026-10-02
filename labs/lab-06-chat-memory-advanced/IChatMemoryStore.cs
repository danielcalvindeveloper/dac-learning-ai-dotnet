using Microsoft.Extensions.AI;

public interface IChatMemoryStore
{
    IReadOnlyList<ChatMessage> GetMessages(
        string sessionId);

    void AddMessage(
        string sessionId,
        ChatMessage message);

    void AddMessages(
        string sessionId,
        IEnumerable<ChatMessage> messages);
}
