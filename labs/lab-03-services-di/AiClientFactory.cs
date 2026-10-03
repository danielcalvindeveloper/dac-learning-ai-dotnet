using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

public static class AiClientFactory
{

    public static IChatClient CreateFromEnvironment()
    {
        LabConfiguration configuration;
        // Cargar la configuración del laboratorio
        try
        {
            configuration = LabConfiguration.Load();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }

        if (string.IsNullOrWhiteSpace(configuration.ApiKey))
        {
            throw new InvalidOperationException(
                $"No se encontró la variable de entorno AI_API_KEY.");
        }

        Console.WriteLine($"Proveedor: {configuration.ProviderName}");
        Console.WriteLine($"Modelo: {configuration.Model}");
        Console.WriteLine();

        if (configuration.Endpoint is null)
        {
            return new OpenAI.Chat.ChatClient(configuration.Model, configuration.ApiKey)
                .AsIChatClient();
        }

        OpenAIClientOptions options = new()
        {
            Endpoint = configuration.Endpoint
        };

        OpenAI.Chat.ChatClient client = new(
            configuration.Model,
            new ApiKeyCredential(configuration.ApiKey),
            options);

        return client.AsIChatClient();
    }
}


