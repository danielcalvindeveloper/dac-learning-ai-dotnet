using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

public static class ChatClientFactory
{
    internal static IChatClient Create(LabConfiguration configuration)
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

        // Conservar metadata nativa antes de que el cliente de invocación continúe.
        return new ChatClientBuilder(client.AsIChatClient())
            .Use(getResponseFunc: async (messages, chatOptions, next, cancellationToken) =>
            {
                ChatResponse response = await next.GetResponseAsync(
                    messages, chatOptions, cancellationToken);

                foreach (ChatMessage message in response.Messages)
                {
                    if (message.RawRepresentation is OpenAI.Chat.ChatCompletion completion)
                    {
                        message.RawRepresentation =
                            new OpenAI.Chat.AssistantChatMessage(completion);
                    }
                }

                return response;
            }, getStreamingResponseFunc: null)
            .Build();
    }
}
