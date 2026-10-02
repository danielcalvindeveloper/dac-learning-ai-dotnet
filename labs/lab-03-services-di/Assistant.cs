using Microsoft.Extensions.AI;

public sealed class Assistant : IAssistant
{
    private readonly IChatClient _chatClient;

    public Assistant(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> ChatAsync(string message)
    {
        ChatResponse response =
            await _chatClient.GetResponseAsync(message);

        return response.Text;
    }

    public async Task<string> TraducirAsync(string texto)
    {
        string prompt =
            $"Traduce al inglés: {texto}";

        ChatResponse response =
            await _chatClient.GetResponseAsync(prompt);

        return response.Text;
    }

    public async Task<string> ResumirAsync(string texto)
    {
        string prompt =
            $"Resume en una frase: {texto}";

        ChatResponse response =
            await _chatClient.GetResponseAsync(prompt);

        return response.Text;
    }
}
