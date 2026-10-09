using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

public static class ChatClientFactory
{
    internal static IChatClient Create(LabConfiguration configuration)
    {
        if (configuration.Endpoint is null)
        {
            return new OpenAI.Chat.ChatClient(
                configuration.Model,
                configuration.ApiKey)
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
