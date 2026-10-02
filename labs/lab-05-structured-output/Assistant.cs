using Microsoft.Extensions.AI;

public sealed class Assistant : IAssistant
{
    private readonly IChatClient _chatClient;

    public Assistant(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<Persona> ExtraerPersonaAsync(string texto)
    {
        string prompt = PromptTemplates.ExtraerPersona(texto);

        ChatResponse<Persona> response =
            await _chatClient.GetResponseAsync<Persona>(prompt);

        return GetResultOrThrow(response);
    }

    public async Task<Personas> ExtraerPersonasAsync(string texto)
    {
        string prompt = PromptTemplates.ExtraerPersonas(texto);

        ChatResponse<Personas> response =
            await _chatClient.GetResponseAsync<Personas>(prompt);

        return GetResultOrThrow(response);
    }

    public async Task<Producto> ExtraerProductoAsync(string texto)
    {
        string prompt = PromptTemplates.ExtraerProducto(texto);

        ChatResponse<Producto> response =
            await _chatClient.GetResponseAsync<Producto>(prompt);

        return GetResultOrThrow(response);
    }

    private static T GetResultOrThrow<T>(ChatResponse<T> response)
    {
        if (response.TryGetResult(out T? result) && result is not null)
        {
            return result;
        }

        throw new InvalidOperationException(
            "El modelo no devolvió una respuesta compatible " +
            $"con el tipo {typeof(T).Name}. " +
            $"Respuesta recibida: {response.Text}");
    }
}
