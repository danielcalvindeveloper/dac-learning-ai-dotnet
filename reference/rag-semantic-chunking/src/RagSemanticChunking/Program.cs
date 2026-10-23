using System.ClientModel;
using System.Text.Json;
using CommunityToolkit.VectorData.Qdrant;
using Grpc.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using OpenAI;
using Qdrant.Client;
using RagSemanticChunking;

// Punto de composición: aquí creamos dependencias y elegimos el comando; los algoritmos viven en clases pequeñas.
// Los logs van a stderr y la salida pedagógica a stdout, para poder distinguir diagnóstico y resultados.
using ILoggerFactory logging = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
}).AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace));
ILogger logger = logging.CreateLogger("Program");
// Ctrl+C solicita cancelación: el mismo token llegará a lectura, llamadas de IA y operaciones de Qdrant.
using CancellationTokenSource cancellation = new();
ConsoleCancelEventHandler cancelHandler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
Console.CancelKeyPress += cancelHandler;
try
{
    // Validamos argumentos antes de abrir conexiones: un error de uso no necesita servicios externos.
    CommandLine command = CommandLine.Parse(args);
    AppConfiguration config = AppConfiguration.Load();
    ChunkingSettings settings = new();
    // El SDK concreto queda en este borde. Las estrategias reciben abstracciones de Microsoft.Extensions.AI.
    // AI_MODEL genera texto; AI_EMBEDDING_MODEL produce vectores. No asumimos que un modelo sirva para ambos.
    OpenAIClientOptions options = new() { RetryPolicy = new QuotaRetryPolicy(logger) };
    if (config.Endpoint is not null) options.Endpoint = config.Endpoint;
    // AsIEmbeddingGenerator adapta el cliente del SDK al contrato común que reciben las estrategias.
    using IEmbeddingGenerator<string, Embedding<float>> generator = new OpenAI.Embeddings.EmbeddingClient(
        config.EmbeddingModel, new ApiKeyCredential(config.ApiKey), options).AsIEmbeddingGenerator();
    // Sólo creamos chat cuando hay decisiones LLM en ingest o una respuesta generada en query.
    using IChatClient? chat = command.Strategy == "llm" && command.Command == "ingest"
        || command.Command == "query" && !command.SearchOnly
        ? new OpenAI.Chat.ChatClient(config.Model, new ApiKeyCredential(config.ApiKey), options).AsIChatClient() : null;
    using QdrantClient client = new(config.QdrantEndpoint, grpcTimeout: TimeSpan.FromSeconds(10));
    // QdrantClient tiene su propio using. ownsClient: false evita que el store también se apropie del cliente.
    // Los using liberan recursos incluso cuando falla una operación o se cancela la ejecución.
    using VectorStore store = new QdrantVectorStore(client, ownsClient: false);
    // El corpus se copia al output y se localiza respecto del ejecutable. Los artifacts pertenecen
    // al directorio de trabajo para conservar manifiestos y reportes entre ejecuciones desde esa carpeta.
    string data = Path.Combine(AppContext.BaseDirectory, "data"), artifacts = Path.GetFullPath("artifacts");
    CancellationToken token = cancellation.Token;
    VectorSearchService search = new(store);
    // Ingest: preparar documentos, decidir cortes, vectorizar chunks finales y publicar una colección.
    if (command.Command == "ingest")
    {
        // Selección explícita de cuatro implementaciones reales: no necesitamos un registry o factory.
        IChunkingStrategy strategy = command.Strategy switch
        {
            "fixed" => new FixedChunkingStrategy(settings),
            "structure" => new StructureAwareChunkingStrategy(settings),
            "semantic" => new EmbeddingSemanticChunkingStrategy(settings, generator),
            "llm" => new LlmSemanticChunkingStrategy(settings, chat!),
            _ => throw new InvalidOperationException("Estrategia no validada.")
        };
        DocumentIngestionService ingestion = new(generator, store, settings, logging.CreateLogger<DocumentIngestionService>());
        IndexManifest manifest = await ingestion.IngestAsync(data, artifacts, strategy, config, token);
        Console.WriteLine($"{manifest.Strategy}: {manifest.Chunks.Length} chunks, colección {ExperimentSettings.Collection(manifest.Strategy)}");
        Console.WriteLine($"Manifiesto y boundaries: {IndexManifest.PathFor(artifacts, manifest.Strategy)}");
    }
    // Compare: reutilizar las cuatro ingestas y medir retrieval, sin pedir respuestas generativas.
    else if (command.Command == "compare")
    {
        List<IndexManifest> manifests = [];
        foreach (string strategy in CommandLine.Strategies) manifests.Add(await IndexManifest.LoadAsync(artifacts, strategy, token));
        IReadOnlyList<DocumentText> corpus = await DocumentIngestionService.LoadCorpusAsync(data, token);
        QuerySet queries = await QuerySet.LoadAsync(Path.Combine(AppContext.BaseDirectory, "examples", "queries.json"), corpus, token);
        ComparisonReport report = await new ComparisonService(generator, search).CompareAsync(queries, manifests, config,
            IndexManifest.HashCorpus(corpus), artifacts, token);
        Console.WriteLine(Evaluation.Markdown(report));
        Console.WriteLine($"Reportes: {Path.Combine(artifacts, "comparison.md")} y comparison.json");
    }
    else
    {
        // Query: validar el snapshot, vectorizar la pregunta y recuperar contexto de una sola estrategia.
        IndexManifest manifest = await IndexManifest.LoadAsync(artifacts, command.Strategy!, token);
        manifest.ValidateConfiguration(config);
        GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(generator, [command.Question], token);
        IReadOnlyList<SearchHit> hits = await search.SearchAsync(embeddings[0].Vector, manifest, token);
        Console.WriteLine($"Pregunta: {command.Question}\nEstrategia: {manifest.Strategy}, topK: {ExperimentSettings.TopK}");
        foreach (SearchHit hit in hits) Console.WriteLine($"score={hit.Score:F4} | {hit.Record.Source}#{hit.Record.ChunkIndex} | [{hit.Record.Start},{hit.Record.End}) | {hit.Record.Section}");
        // --search-only permite inspeccionar retrieval sin consumir una llamada al modelo generativo.
        if (!command.SearchOnly)
        {
            RagAnswer answer = await new RagService(chat!).AnswerAsync(command.Question, hits, token);
            Console.WriteLine($"\n=== RESPUESTA ===\n{answer.Text}\n\nFuentes: {string.Join(", ", answer.Sources)}\nChat invocado: {answer.UsedModel}");
        }
    }
}
catch (Exception) when (cancellation.IsCancellationRequested)
{
    logger.LogWarning("Operación cancelada"); Environment.ExitCode = 130;
}
catch (Exception ex)
{
    // El borde traduce errores sin volcar respuestas remotas, prompts o credenciales.
    // Un fallo gRPC puede estar envuelto por el conector; recorremos las causas para dar un diagnóstico útil.
    RpcException? rpc = null;
    for (Exception? current = ex; current is not null; current = current.InnerException) rpc ??= current as RpcException;
    string message = rpc is not null ? $"Qdrant ({rpc.StatusCode}): revisá servicio, gRPC, colección y dimensión."
        : ex is ClientResultException remote ? $"Proveedor AI: HTTP {remote.Status}. Revisá modelos, Structured Output, endpoint y cuota."
        : ex is HttpRequestException ? "No se pudo contactar al proveedor AI."
        : ex is JsonException ? "JSON inválido en manifiesto o queries. Revisá los archivos."
        : ex is InvalidOperationException or ArgumentException or IOException or NotSupportedException ? ex.Message
        : $"Error inesperado ({ex.GetType().Name}). Revisá configuración y la guía de diagnóstico.";
    logger.LogError("{Message}", message); Environment.ExitCode = 1;
}
// Quitamos la suscripción al evento al salir para completar la gestión de recursos del proceso.
finally { Console.CancelKeyPress -= cancelHandler; }
