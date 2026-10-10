using System.ClientModel;
using System.Text.Json;
using CommunityToolkit.VectorData.Qdrant;
using Grpc.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using OpenAI;
using Qdrant.Client;
using RagHybridSearch;

using ILoggerFactory logging = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
}).AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace));
ILogger logger = logging.CreateLogger("Program");
using CancellationTokenSource cancellation = new();
ConsoleCancelEventHandler cancelHandler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
Console.CancelKeyPress += cancelHandler;
try
{
    CommandLine command = CommandLine.Parse(args);
    AppConfiguration configuration = AppConfiguration.Load();
    OpenAIClientOptions options = new();
    if (configuration.Endpoint is not null) options.Endpoint = configuration.Endpoint;
    string indexPath = Path.GetFullPath(Path.Combine("artifacts", "lucene-index"));
    // Inyección por constructor: Program compone servicios, sin contenedor ni interfaces propias.
    // BM25 con --search-only no crea clientes de IA ni Qdrant, y puede ejecutarse offline.
    using IEmbeddingGenerator<string, Embedding<float>>? generator = command.Ingest || command.Mode != SearchMode.Bm25
        ? new OpenAI.Embeddings.EmbeddingClient(configuration.EmbeddingModel,
            new ApiKeyCredential(configuration.ApiKey), options).AsIEmbeddingGenerator() : null;
    using QdrantClient? qdrant = generator is not null ? new(configuration.QdrantEndpoint, grpcTimeout: TimeSpan.FromSeconds(10)) : null;
    using VectorStore? store = qdrant is not null ? new QdrantVectorStore(qdrant, ownsClient: false) : null;
    if (command.Ingest)
    {
        DocumentIngestionService ingestion = new(generator!, store!, logging.CreateLogger<DocumentIngestionService>());
        await ingestion.IngestAsync(Path.Combine(AppContext.BaseDirectory, "data"), indexPath, configuration.EmbeddingModel, cancellation.Token);
    }
    else
    {
        using LexicalSearchService lexical = new(indexPath);
        IndexMetadata metadata = lexical.Metadata;
        Console.WriteLine($"Índice Lucene persistido: {lexical.Count} documentos");
        if (command.Mode != SearchMode.Bm25 && metadata.EmbeddingModel != configuration.EmbeddingModel)
            throw new InvalidOperationException("AI_EMBEDDING_MODEL difiere del modelo indexado. Revisá la configuración.");
        HybridSearchService search = new(command.Mode == SearchMode.Vector ? null : lexical,
            generator is not null ? new VectorSearchService(generator, store!) : null);
        RetrievalResult result = await search.SearchAsync(command.Question, command.Mode, command.Filter, metadata, cancellation.Token);
        Console.WriteLine($"Pregunta: {command.Question}\nFiltro: {command.Filter}\nTopK vector/BM25/final: {SearchSettings.TopKVector}/{SearchSettings.TopKBm25}/{SearchSettings.TopKFinal}");
        if (command.Mode != SearchMode.Bm25) PrintRanking("VECTOR SEARCH", result.Vector);
        if (command.Mode != SearchMode.Vector) PrintRanking("BM25", result.Bm25);
        Console.WriteLine(command.Mode == SearchMode.Hybrid ? "\n=== HYBRID (RRF) ===" : "\n=== RANKING FINAL ===");
        for (int i = 0; i < result.Final.Count; i++)
        {
            FusedHit hit = result.Final[i];
            Console.WriteLine($"{i + 1}. score={hit.Score:F6} vectorRank={hit.VectorRank} bm25Rank={hit.Bm25Rank} | {Describe(hit.Record)}");
        }
        if (!command.SearchOnly)
        {
            using IChatClient chat = new OpenAI.Chat.ChatClient(configuration.Model,
                new ApiKeyCredential(configuration.ApiKey), options).AsIChatClient();
            RagService rag = new(chat, logging.CreateLogger<RagService>());
            RagAnswer answer = await rag.AnswerAsync(command.Question, result.Final, cancellation.Token);
            Console.WriteLine($"\n=== RESPUESTA ===\n{answer.Text}\nModelo generativo invocado: {answer.UsedModel}");
            Console.WriteLine("\n=== FUENTES DEL CONTEXTO ENVIADO ===");
            foreach (string source in answer.Sources) Console.WriteLine($"- {source}");
        }
    }
}
catch (Exception) when (cancellation.IsCancellationRequested)
{
    logger.LogWarning("Operación cancelada");
    Environment.ExitCode = 130;
}
catch (Exception ex)
{
    // No registramos excepciones crudas: pueden contener respuestas de servicios o datos sensibles.
    RpcException? rpc = null;
    for (Exception? current = ex; current is not null; current = current.InnerException) rpc ??= current as RpcException;
    string message = rpc is not null ? $"Qdrant ({rpc.StatusCode}): revisá gRPC, colección y dimensión; ejecutá docker compose ps."
        : ex is ClientResultException client ? $"Proveedor AI: HTTP {client.Status}. Revisá configuración y cuota."
        : ex is HttpRequestException ? "No se pudo contactar al proveedor AI."
        : ex is JsonException ? "JSON inválido en data/metadata.json. Revisá el archivo e ingestá nuevamente."
        : ex is Lucene.Net.Store.LockObtainFailedException ? "El índice Lucene está bloqueado por otro escritor. Esperá a que termine la otra ingesta."
        : ex is Lucene.Net.Index.CorruptIndexException ? "El índice Lucene está dañado. Revisá el almacenamiento y la guía de reconstrucción."
        : ex is InvalidOperationException or ArgumentException or IOException or NotSupportedException ? ex.Message
        : $"Error inesperado ({ex.GetType().Name}). Revisá configuración e índices.";
    logger.LogError("{Message}", message);
    Environment.ExitCode = 1;
}
finally { Console.CancelKeyPress -= cancelHandler; }

static string Describe(DocumentRecord record) => $"source={record.Source} chunk={record.ChunkIndex} department={record.Department} type={record.DocumentType} year={record.Year}";
static void PrintRanking(string title, IReadOnlyList<SearchHit> hits)
{
    Console.WriteLine($"\n=== {title} ===");
    for (int i = 0; i < hits.Count; i++) Console.WriteLine($"{i + 1}. score={hits[i].Score:F6} | {Describe(hits[i].Record)}");
}
