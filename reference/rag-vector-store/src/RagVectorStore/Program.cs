using System.ClientModel;
using CommunityToolkit.VectorData.Qdrant;
using Grpc.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OpenAI;
using Qdrant.Client;
using RagVectorStore;

if (args.Length == 0 || (args[0] != "ingest" && args[0] != "query")
    || (args[0] == "ingest" && args.Length != 1)
    || (args[0] == "query" && (args.Length < 2 || string.IsNullOrWhiteSpace(string.Join(' ', args.Skip(1))))))
{
    Console.WriteLine("Uso: dotnet run --project src/RagVectorStore -- ingest");
    Console.WriteLine("     dotnet run --project src/RagVectorStore -- query \"Tu pregunta\"");
    Environment.ExitCode = 1;
    return;
}

// Ctrl+C solicita cancelación; las operaciones reciben el token y pueden liberar sus recursos.
using CancellationTokenSource cancellation = new();
ConsoleCancelEventHandler cancelHandler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
Console.CancelKeyPress += cancelHandler;
try
{
    AppConfiguration configuration = AppConfiguration.Load();
    OpenAIClientOptions options = new();
    if (configuration.Endpoint is not null)
    {
        options.Endpoint = configuration.Endpoint;
    }

    // Adaptamos el SDK concreto: los servicios trabajan con las abstracciones de Microsoft.Extensions.AI.
    using IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OpenAI.Embeddings.EmbeddingClient(
        configuration.EmbeddingModel, new ApiKeyCredential(configuration.ApiKey), options).AsIEmbeddingGenerator();
    using QdrantClient qdrant = new(configuration.QdrantEndpoint, grpcTimeout: TimeSpan.FromSeconds(10));
    // Program conserva la propiedad del cliente gRPC y lo libera con using, después del store.
    using VectorStore vectorStore = new QdrantVectorStore(qdrant, ownsClient: false);

    Console.WriteLine($"Proveedor: {configuration.ProviderName} | Embeddings: {configuration.EmbeddingModel}");
    Console.WriteLine($"Qdrant (gRPC): {configuration.QdrantEndpoint} | Colección: {RagSettings.CollectionName}");

    if (args[0] == "ingest")
    {
        // Verificamos conectividad antes de consumir embeddings pagos.
        await vectorStore.CollectionExistsAsync(RagSettings.CollectionName, cancellation.Token);
        IngestionService ingestion = new(embeddingGenerator, vectorStore);
        // El .csproj copia el corpus junto al ejecutable; no dependemos del directorio actual para leerlo.
        await ingestion.IngestAsync(Path.Combine(AppContext.BaseDirectory, "data"), cancellation.Token);
    }
    else
    {
        string question = string.Join(' ', args.Skip(1));
        VectorSearchService search = new(embeddingGenerator, vectorStore);
        IReadOnlyList<SearchHit> hits = await search.SearchAsync(question, cancellation.Token);
        Console.WriteLine($"\n=== RETRIEVAL ===\nPregunta: {question}");
        Console.WriteLine($"Top K: {RagSettings.TopK} | Score mínimo experimental: {RagSettings.MinimumScore:F2}");
        foreach (SearchHit hit in hits)
        {
            Console.WriteLine($"score: {hit.Score:F4} | source: {hit.Record.Source} | document: {hit.Record.DocumentId} | chunk: {hit.Record.ChunkIndex}");
        }

        // Crear el cliente no envía una petición; RagService sólo llama al chat si acepta evidencia.
        using IChatClient chatClient = new OpenAI.Chat.ChatClient(
            configuration.Model, new ApiKeyCredential(configuration.ApiKey), options).AsIChatClient();
        RagService rag = new(chatClient);
        RagAnswer answer = await rag.AnswerAsync(question, hits, cancellation.Token);
        Console.WriteLine($"\n=== RESPUESTA ===\n{answer.Text}");
        Console.WriteLine($"Modelo generativo invocado: {answer.UsedModel} ({configuration.Model})");
        Console.WriteLine("\n=== FUENTES DEL CONTEXTO ENVIADO ===");
        foreach (string source in answer.Sources)
        {
            Console.WriteLine($"- {source}");
        }
    }
}
catch (Exception) when (cancellation.IsCancellationRequested)
{
    Console.Error.WriteLine("Operación cancelada.");
    Environment.ExitCode = 130;
}
catch (Exception ex)
{
    // Borde de la aplicación: los SDK pueden envolver errores gRPC. No volcamos respuestas ni credenciales.
    RpcException? rpc = null;
    for (Exception? current = ex; current is not null; current = current.InnerException)
    {
        rpc ??= current as RpcException;
    }

    string message = rpc?.StatusCode switch
    {
        StatusCode.Unavailable or StatusCode.DeadlineExceeded =>
            "Qdrant no está disponible. Ejecutá docker compose up -d y revisá QDRANT_ENDPOINT (gRPC 6334).",
        StatusCode.InvalidArgument =>
            "Qdrant rechazó el vector o el esquema. Verificá que la colección use la misma dimensión y modelo que la ingesta.",
        StatusCode.NotFound => "No se encontró la colección en Qdrant. Ejecutá ingest.",
        not null => $"Falló Qdrant ({rpc.StatusCode}). Revisá docker compose logs qdrant.",
        _ when ex is ClientResultException client => $"Falló el proveedor AI (HTTP {client.Status}). Revisá la configuración y la conectividad.",
        _ when ex is HttpRequestException => "No se pudo contactar al proveedor AI. Revisá AI_URL y la conexión.",
        _ when ex is InvalidOperationException or ArgumentException or IOException or NotSupportedException => ex.Message,
        _ => $"Error inesperado ({ex.GetType().Name}). Revisá el .env y la configuración del entorno."
    };
    Console.Error.WriteLine(message);
    Environment.ExitCode = 1;
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
}
