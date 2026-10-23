using DotNetEnv;

namespace RagSemanticChunking;

/// <summary>Configuración externa validada, compartida por ingesta, consulta y comparación.</summary>
/// <param name="ProviderName">Identidad del proveedor; la integración usa un endpoint OpenAI-compatible.</param>
/// <param name="ApiKey">Credencial sensible: no debe incluirse en logs ni manifiestos.</param>
/// <param name="Model">AI_MODEL: modelo generativo para decidir límites LLM o redactar respuestas.</param>
/// <param name="EmbeddingModel">AI_EMBEDDING_MODEL: modelo que transforma textos en vectores.</param>
/// <param name="Endpoint">AI_URL opcional; null conserva el endpoint del SDK.</param>
/// <param name="QdrantEndpoint">Dirección gRPC de Qdrant, distinta de su endpoint REST.</param>
public sealed record AppConfiguration(
    string ProviderName,
    string ApiKey,
    string Model,
    string EmbeddingModel,
    Uri? Endpoint,
    Uri QdrantEndpoint)
{
    /// <summary>Carga el .env más cercano recorriendo directorios padres y valida sus valores.</summary>
    public static AppConfiguration Load()
    {
        // La búsqueda del .env parte del directorio de trabajo y asciende hacia la raíz.
        Env.TraversePath().Load();
        return Parse(Environment.GetEnvironmentVariable);
    }

    /// <summary>Valida valores sin acoplar la lógica al entorno del proceso.</summary>
    /// <param name="read">Función que obtiene una variable; los tests pueden suministrar un diccionario.</param>
    /// <exception cref="InvalidOperationException">Falta una variable obligatoria o una URL es inválida.</exception>
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

    // Validar antes de crear clientes permite dar errores de configuración concretos.
    // Se rechazan credenciales en la URL para evitar exponerlas accidentalmente en diagnósticos.
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
