using DotNetEnv;

namespace RagHybridSearch;

public sealed record AppConfiguration(
    string ProviderName,
    string ApiKey,
    string Model,
    string EmbeddingModel,
    Uri? Endpoint,
    Uri QdrantEndpoint)
{
    public static AppConfiguration Load()
    {
        // La búsqueda del .env parte del directorio de trabajo y asciende hacia la raíz.
        Env.TraversePath().Load();
        return Parse(Environment.GetEnvironmentVariable);
    }

    public static AppConfiguration Parse(Func<string, string?> read)
    {
        // Recibir la lectura como función permite probar la validación con un diccionario, sin credenciales.
        ArgumentNullException.ThrowIfNull(read);

        return new AppConfiguration(
            Required(read, "AI_PROVIDER"),
            Required(read, "AI_API_KEY"),
            Required(read, "AI_MODEL"),
            Required(read, "AI_EMBEDDING_MODEL"),
            EndpointUri(read("AI_URL"), "AI_URL"),
            EndpointUri(read("QDRANT_ENDPOINT"), "QDRANT_ENDPOINT")
                ?? new Uri("http://localhost:6334"));
    }

    private static string Required(Func<string, string?> read, string name)
    {
        string? value = read(name);
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Falta configurar {name} en el .env de la raíz.")
            : value.Trim();
    }

    private static Uri? EndpointUri(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException($"{name} debe ser una URL HTTP/HTTPS válida, sin credenciales.");
        }

        if (name == "QDRANT_ENDPOINT"
            && (uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0))
        {
            throw new InvalidOperationException("QDRANT_ENDPOINT debe indicar host y puerto gRPC, sin ruta ni query.");
        }

        return uri;
    }
}
