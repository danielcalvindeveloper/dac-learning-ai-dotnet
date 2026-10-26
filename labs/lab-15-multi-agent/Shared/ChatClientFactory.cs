using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

internal static class ChatClientFactory
{
    public static IChatClient Create(LabConfiguration configuration)
    {
        OpenAIClientOptions options = new();

        if (configuration.Endpoint is not null)
        {
            options.Endpoint = configuration.Endpoint;
        }

        OpenAI.Chat.ChatClient client = new(
            configuration.Model,
            new ApiKeyCredential(configuration.ApiKey),
            options);

        return client.AsIChatClient();
    }
}
