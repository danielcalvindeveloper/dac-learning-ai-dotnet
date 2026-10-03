using DotNetEnv;

namespace Lab01.HelloLlm;

internal sealed record LabConfiguration(
    string ProviderName,
    string ApiKey,
    string Model,
    Uri? Endpoint)
{
    public static LabConfiguration Load()
    {
        Env.TraversePath().Load();

        string provider = GetRequired("AI_PROVIDER");
        string model = GetRequired("AI_MODEL");
        string apiKey = GetRequired("AI_API_KEY");

        string? url =
            Environment.GetEnvironmentVariable("AI_URL");

        Uri? endpoint = GetOptionalEndpoint(url);

        return new LabConfiguration(
            ProviderName: provider,
            ApiKey: apiKey,
            Model: model,
            Endpoint: endpoint);
    }

    private static string GetRequired(string variableName)
    {
        string? value =
            Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Falta configurar la variable {variableName}.");
        }

        return value.Trim();
    }

    private static Uri? GetOptionalEndpoint(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(
                url.Trim(),
                UriKind.Absolute,
                out Uri? endpoint))
        {
            throw new InvalidOperationException(
                "La variable AI_URL no contiene una URL válida.");
        }

        return endpoint;
    }
}
