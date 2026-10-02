using DotNetEnv;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

const string DefaultProvider = "openai";
const string DefaultOpenAiModel = "gpt-4o-mini";
const string DefaultGeminiModel = "gemini-3.5-flash-lite";
const string DefaultOpenRouterModel = "openrouter/free";

// Busca el .env común del proyecto recorriendo los directorios padres.
Env.TraversePath().Load();

string provider = (Environment.GetEnvironmentVariable("AI_PROVIDER") ?? DefaultProvider)
    .Trim()
    .ToLowerInvariant();

ProviderConfiguration config;

try
{
    // Resuelve la API key, el modelo por defecto y el endpoint
    // necesarios para conectarse al proveedor seleccionado.
    config = ResolveProviderConfiguration(provider);
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return;
}

string? apiKey = Environment.GetEnvironmentVariable(config.ApiKeyVariable);
string model = Environment.GetEnvironmentVariable("AI_MODEL") ?? config.DefaultModel;

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine(
        $"Error: no se encontró la variable de entorno {config.ApiKeyVariable}.");
    return;
}

IChatClient chatClient = CreateChatClient(config, model, apiKey);

Console.WriteLine($"Proveedor: {config.Name}");
Console.WriteLine($"Modelo: {model}");
Console.WriteLine();

const string firstMessage = "Mi nombre es Daniel";
const string secondMessage = "¿Cómo me llamo?";

Console.WriteLine("=== SIN MEMORIA ===");

Console.WriteLine($"Usuario: {firstMessage}");
ChatResponse r1 = await chatClient.GetResponseAsync(firstMessage);
Console.WriteLine($"Asistente: {r1.Text}");
Console.WriteLine();

Console.WriteLine($"Usuario: {secondMessage}");
ChatResponse r2 = await chatClient.GetResponseAsync(secondMessage);
Console.WriteLine($"Asistente: {r2.Text}");
Console.WriteLine();

Console.WriteLine("=== CON MEMORIA ===");

List<ChatMessage> history = [];

await ChatWithMemoryAsync(
    firstMessage,
    history,
    chatClient);

await ChatWithMemoryAsync(
    secondMessage,
    history,
    chatClient);

static async Task ChatWithMemoryAsync(
    string message,
    List<ChatMessage> history,
    IChatClient chatClient)
{
    Console.WriteLine($"Usuario: {message}");

    history.Add(
        new ChatMessage(ChatRole.User, message));

    ChatResponse response =
        await chatClient.GetResponseAsync(history);

    Console.WriteLine($"Asistente: {response.Text}");
    Console.WriteLine();

    foreach (ChatMessage responseMessage in response.Messages)
    {
        history.Add(responseMessage);
    }
}

static ProviderConfiguration ResolveProviderConfiguration(string provider)
{
    return provider switch
    {
        "openai" => new ProviderConfiguration(
            Name: "OpenAI",
            ApiKeyVariable: "OPENAI_API_KEY",
            DefaultModel: DefaultOpenAiModel,
            Endpoint: null),

        "gemini" => new ProviderConfiguration(
            Name: "Gemini",
            ApiKeyVariable: "GEMINI_API_KEY",
            DefaultModel: DefaultGeminiModel,
            Endpoint: new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")),

        "openrouter" => new ProviderConfiguration(
            Name: "OpenRouter",
            ApiKeyVariable: "OPENROUTER_API_KEY",
            DefaultModel: DefaultOpenRouterModel,
            Endpoint: new Uri("https://openrouter.ai/api/v1")),

        _ => throw new InvalidOperationException(
            $"Proveedor no reconocido: '{provider}'. Usa openai, gemini u openrouter.")
    };
}

static IChatClient CreateChatClient(
    ProviderConfiguration config,
    string model,
    string apiKey)
{
    if (config.Endpoint is null)
    {
        return new OpenAI.Chat.ChatClient(model, apiKey).AsIChatClient();
    }

    OpenAIClientOptions options = new()
    {
        Endpoint = config.Endpoint
    };

    OpenAI.Chat.ChatClient client = new(
        model,
        new ApiKeyCredential(apiKey),
        options);

    return client.AsIChatClient();
}

internal sealed record ProviderConfiguration(
    string Name,
    string ApiKeyVariable,
    string DefaultModel,
    Uri? Endpoint);
