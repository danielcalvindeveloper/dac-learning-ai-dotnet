using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

public static class AiClientFactory
{
    private const string DefaultProvider = "openai";
    private const string DefaultOpenAiModel = "gpt-4o-mini";
    private const string DefaultGeminiModel = "gemini-3.5-flash-lite";
    private const string DefaultOpenRouterModel = "openrouter/free";

    public static IChatClient CreateFromEnvironment()
    {
        string provider =
            (Environment.GetEnvironmentVariable("AI_PROVIDER") ?? DefaultProvider)
            .Trim()
            .ToLowerInvariant();

        ProviderConfiguration config = ResolveProviderConfiguration(provider);

        string? apiKey = Environment.GetEnvironmentVariable(config.ApiKeyVariable);
        string model = Environment.GetEnvironmentVariable("AI_MODEL") ?? config.DefaultModel;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"No se encontró la variable de entorno {config.ApiKeyVariable}.");
        }

        Console.WriteLine($"Proveedor: {config.Name}");
        Console.WriteLine($"Modelo: {model}");
        Console.WriteLine();

        if (config.Endpoint is null)
        {
            return new OpenAI.Chat.ChatClient(model, apiKey).AsIChatClient();
        }

        OpenAIClientOptions options = new() { Endpoint = config.Endpoint };

        OpenAI.Chat.ChatClient client = new(
            model,
            new ApiKeyCredential(apiKey),
            options);

        return client.AsIChatClient();
    }

    private static ProviderConfiguration ResolveProviderConfiguration(string provider)
    {
        return provider switch
        {
            "openai" => new ProviderConfiguration(
                "OpenAI", "OPENAI_API_KEY", DefaultOpenAiModel, null),

            "gemini" => new ProviderConfiguration(
                "Gemini", "GEMINI_API_KEY", DefaultGeminiModel,
                new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")),

            "openrouter" => new ProviderConfiguration(
                "OpenRouter", "OPENROUTER_API_KEY", DefaultOpenRouterModel,
                new Uri("https://openrouter.ai/api/v1")),

            _ => throw new InvalidOperationException(
                $"Proveedor no reconocido: '{provider}'. Usa openai, gemini u openrouter.")
        };
    }

    private sealed record ProviderConfiguration(
        string Name,
        string ApiKeyVariable,
        string DefaultModel,
        Uri? Endpoint);
}
