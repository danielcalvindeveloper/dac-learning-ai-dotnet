public interface IAssistant
{
    Task<string> ChatAsync(
        string sessionId,
        string message);
}
