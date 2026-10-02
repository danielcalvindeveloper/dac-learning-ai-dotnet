using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

const string DefaultProvider = "openai";
const string DefaultOpenAiModel = "gpt-4o-mini";
const string DefaultGeminiModel = "gemini-3.8-flash";
const string DefaultOpenRouterModel = "openrouter/free";

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
    Console.Error.WriteLine(
        $"Configúrala antes de ejecutar el laboratorio con AI_PROVIDER={provider}.");
    return;
}

IChatClient chatClient = CreateChatClient(config, model, apiKey);

const string prompt = "Explica en una sola oración qué es un modelo de lenguaje.";

Console.WriteLine($"Proveedor: {config.Name}");
Console.WriteLine($"Modelo: {model}");
Console.WriteLine($"Prompt: {prompt}");
Console.WriteLine();
Console.WriteLine("Respuesta:");

ChatResponse response = await chatClient.GetResponseAsync(prompt);

Console.WriteLine(response.Text);

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
        return new ChatClient(model, apiKey).AsIChatClient();
    }

    OpenAIClientOptions options = new()
    {
        Endpoint = config.Endpoint
    };

    ChatClient client = new(
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
