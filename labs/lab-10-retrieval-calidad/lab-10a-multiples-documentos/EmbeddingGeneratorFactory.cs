using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Embeddings;
using System.ClientModel;

public static class EmbeddingGeneratorFactory
{
    internal static IEmbeddingGenerator<string, Embedding<float>> Create(
        LabConfiguration configuration)
    {
        if (configuration.Endpoint is null)
        {
            return new EmbeddingClient(
                configuration.EmbeddingModel,
                configuration.ApiKey)
                .AsIEmbeddingGenerator();
        }

        OpenAIClientOptions options = new()
        {
            Endpoint = configuration.Endpoint
        };

        EmbeddingClient client = new(
            configuration.EmbeddingModel,
            new ApiKeyCredential(configuration.ApiKey),
            options);

        return client.AsIEmbeddingGenerator();
    }
}
