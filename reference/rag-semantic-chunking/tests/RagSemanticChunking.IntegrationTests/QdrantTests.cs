using CommunityToolkit.VectorData.Qdrant;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;
using Xunit;

namespace RagSemanticChunking.IntegrationTests;

// Proyecto separado: infraestructura real con IA controlada, nunca Docker en la suite offline.
/// <summary>Comprueba ingesta, reingesta, retrieval y coherencia contra Qdrant real.</summary>
/// <remarks>Se ejecuta aparte de los unit tests; la IA permanece controlada para evitar red externa y cuotas.</remarks>
public sealed class QdrantTests
{
    [Fact]
    public async Task FourStrategiesIngestReingestRetrieveAndCompareInIsolatedCollections()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        // Un prefijo aleatorio impide tocar colecciones del ejemplo o de otras ejecuciones de pruebas.
        string prefix = "chunking_test_" + Guid.NewGuid().ToString("N") + "_";
        string directory = Path.Combine(Path.GetTempPath(), prefix), data = Path.Combine(directory, "data"), artifacts = Path.Combine(directory, "artifacts");
        Directory.CreateDirectory(data);
        Uri endpoint = new(Environment.GetEnvironmentVariable("QDRANT_ENDPOINT") ?? "http://localhost:6334");
        AppConfiguration config = new("test", "not-used", "test-chat", "test-vector", null, endpoint);
        ChunkingSettings settings = new(MaxSize: 80, MinSize: 10, Overlap: 5, UnitMaxSize: 40);
        using QdrantClient client = new(endpoint, grpcTimeout: TimeSpan.FromSeconds(10));
        using VectorStore store = new QdrantVectorStore(client, ownsClient: false);
        using ConstantEmbeddings generator = new();
        using ProposalChat chat = new();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(data, "a.md"), new string('a', 30) + "\n\n" + new string('b', 30) + "\n\n" + new string('c', 30), token);
            IChunkingStrategy[] strategies = [new FixedChunkingStrategy(settings), new StructureAwareChunkingStrategy(settings),
                new EmbeddingSemanticChunkingStrategy(settings, generator), new LlmSemanticChunkingStrategy(settings, chat)];
            DocumentIngestionService ingestion = new(generator, store, settings, NullLogger<DocumentIngestionService>.Instance, prefix);
            List<IndexManifest> manifests = [];
            VectorSearchService search = new(store, prefix);
            foreach (IChunkingStrategy strategy in strategies)
            {
                IndexManifest first = await ingestion.IngestAsync(data, artifacts, strategy, config, token);
                // Reingerir debe conservar los IDs y reemplazar puntos, en lugar de duplicar el contenido.
                IndexManifest second = await ingestion.IngestAsync(data, artifacts, strategy, config, token);
                Assert.Equal(first.Ids, second.Ids);
                IReadOnlyList<SearchHit> hits = await search.SearchAsync(new float[] { 1, 0 }, second, token);
                Assert.Equal(second.Chunks.Length, hits.Count);
                Assert.All(hits, h =>
                {
                    Assert.Equal(strategy.Name, h.Record.ChunkingStrategy);
                    Assert.Equal("a.md", h.Record.DocumentId);
                    Assert.Equal(h.Record.End - h.Record.Start, h.Record.Text.Length);
                });
                manifests.Add(second);
            }
            QuerySet queries = new(1, [new("Q1", "a", ["a.md"])]);
            ComparisonReport report = await new ComparisonService(generator, search).CompareAsync(queries, manifests,
                config, manifests[0].CorpusHash, artifacts, token);
            Assert.All(report.Strategies, s => Assert.Equal(1, s.MeanRecall));
            Assert.True(File.Exists(Path.Combine(artifacts, "comparison.json")));
            using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(prefix + "llm", DocumentRecord.Definition(2));
            // Simulamos un punto de otra publicación: el lector debe rechazarlo aunque su vector sea válido.
            DocumentRecord stale = DocumentRecord.Create(manifests[^1].Chunks[0], "llm", "stale-generation", new float[] { 1, 0 });
            await collection.UpsertAsync(stale, token);
            await Assert.ThrowsAsync<InvalidOperationException>(() => search.SearchAsync(new float[] { 1, 0 }, manifests[^1], token));
        }
        finally
        {
            // Sólo colecciones aleatorias y directorio temporal creados por esta prueba.
            foreach (string strategy in CommandLine.Strategies)
            {
                using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(prefix + strategy, DocumentRecord.Definition(2));
                await collection.EnsureCollectionDeletedAsync(CancellationToken.None);
            }
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Vectores iguales para que esta prueba mida persistencia y contratos, no calidad de ranking.</summary>
    private sealed class ConstantEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(_ => new Embedding<float>(new float[] { 1, 0 }))));
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    /// <summary>Elección fija válida para el pequeño corpus de prueba; no llama a un proveedor generativo.</summary>
    private sealed class ProposalChat : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
                """{"EndUnit":2}""")));
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
