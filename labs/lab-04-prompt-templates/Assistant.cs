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

    public async Task<string> TraducirAsync(
        string idioma,
        string texto)
    {
        string prompt =
            PromptTemplates.Traducir(
                idioma,
                texto);

        ChatResponse response =
            await _chatClient.GetResponseAsync(prompt);

        return response.Text;
    }

    public async Task<string> ResumirAsync(
        int lineas,
        string texto)
    {
        string prompt =
            PromptTemplates.Resumir(
                lineas,
                texto);

        ChatResponse response =
            await _chatClient.GetResponseAsync(prompt);

        return response.Text;
    }

    public async Task<string> ConsultarAsync(
        string rol,
        string pregunta)
    {
        string prompt =
            PromptTemplates.Consultar(
                rol,
                pregunta);

        ChatResponse response =
            await _chatClient.GetResponseAsync(prompt);

        return response.Text;
    }
}
