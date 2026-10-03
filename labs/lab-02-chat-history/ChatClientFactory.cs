using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Lab02.ChatHistory;

internal static class ChatClientFactory
{
    public static IChatClient Create(
        LabConfiguration configuration)
    {
        if (configuration.Endpoint is null)
        {
            return new ChatClient(
                configuration.Model,
                configuration.ApiKey)
                .AsIChatClient();
        }

        OpenAIClientOptions options = new()
        {
            Endpoint = configuration.Endpoint
        };

        ChatClient client = new(
            configuration.Model,
            new ApiKeyCredential(configuration.ApiKey),
            options);

        return client.AsIChatClient();
    }
}
