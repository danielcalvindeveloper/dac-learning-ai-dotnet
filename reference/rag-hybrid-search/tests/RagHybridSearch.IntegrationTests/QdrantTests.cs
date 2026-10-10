using CommunityToolkit.VectorData.Qdrant;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;
using Microsoft.Extensions.AI;
using Xunit;

namespace RagHybridSearch.IntegrationTests;

// Proyecto fuera de la solución principal: nunca conecta Docker durante los unit tests.
public sealed class QdrantTests
{
    [Fact]
    public async Task NativeFiltersPersistenceAndHybridCompositionUseBothRealIndexes()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        Uri endpoint = new(Environment.GetEnvironmentVariable("QDRANT_ENDPOINT") ?? "http://localhost:6334");
        string name = "hybrid_test_" + Guid.NewGuid().ToString("N");
        string indexPath = Path.Combine(Path.GetTempPath(), name);
        IndexMetadata metadata = new("controlled-embedding", 2, Guid.NewGuid().ToString("D"));
        using QdrantClient client = new(endpoint, grpcTimeout: TimeSpan.FromSeconds(10));
        using VectorStore store = new QdrantVectorStore(client, ownsClient: false);
        using VectorStoreCollection<Guid, DocumentRecord> collection = store.GetCollection<Guid, DocumentRecord>(name, DocumentRecord.Definition(2));
        try
        {
            await collection.EnsureCollectionExistsAsync(token);
            DocumentRecord nearestButExcluded = DocumentRecord.Create(new("TI", "a.md", 0), new("TI", "policy", 2026), new float[] { 1, 0 });
            DocumentRecord accepted = DocumentRecord.Create(new("RRHH vigente", "b.md", 0), new("RRHH", "policy", 2026), new float[] { .8f, .6f });
            DocumentRecord old = DocumentRecord.Create(new("RRHH anterior", "c.md", 0), new("RRHH", "policy", 2025), new float[] { 1, 0 });
            foreach (DocumentRecord record in new[] { nearestButExcluded, accepted, old }) record.Generation = metadata.Generation;
            await collection.UpsertAsync([nearestButExcluded, accepted, old], token);
            await collection.UpsertAsync(accepted, token);
            using QdrantClient reconnected = new(endpoint, grpcTimeout: TimeSpan.FromSeconds(10));
            using VectorStore reopenedStore = new QdrantVectorStore(reconnected, ownsClient: false);
            using VectorStoreCollection<Guid, DocumentRecord> reopened = reopenedStore.GetCollection<Guid, DocumentRecord>(name, DocumentRecord.Definition(2));
            List<VectorSearchResult<DocumentRecord>> results = [];
            await foreach (VectorSearchResult<DocumentRecord> result in reopened.SearchAsync(new ReadOnlyMemory<float>([1, 0]), 1,
                new VectorSearchOptions<DocumentRecord> { Filter = new MetadataFilter("RRHH", "policy", 2026).ToExpression() }, token))
                results.Add(result);
            Assert.Equal(accepted.Id, Assert.Single(results).Record.Id);
            Assert.Equal("RRHH vigente", results[0].Record.Text);
            results.Clear();
            await foreach (VectorSearchResult<DocumentRecord> result in reopened.SearchAsync(new ReadOnlyMemory<float>([1, 0]), 10,
                new VectorSearchOptions<DocumentRecord> { Filter = new MetadataFilter().ToExpression() }, token)) results.Add(result);
            Assert.Equal(3, results.Count);

            LuceneIndex.Rebuild(indexPath, [nearestButExcluded, accepted, old], metadata, token);
            using LexicalSearchService lexical = new(indexPath);
            using ControlledEmbeddingGenerator generator = new();
            VectorSearchService vector = new(generator, reopenedStore, name);
            RetrievalResult hybrid = await new HybridSearchService(lexical, vector).SearchAsync("RRHH", SearchMode.Hybrid,
                new("RRHH", "policy", 2026), lexical.Metadata, token);
            FusedHit fused = Assert.Single(hybrid.Final);
            Assert.Equal(accepted.Id, fused.Record.Id);
            Assert.Equal(1, fused.VectorRank);
            Assert.Equal(1, fused.Bm25Rank);
            Assert.Equal(["b.md"], RagContext.Build(hybrid.Final).Sources);

            accepted.Generation = Guid.NewGuid().ToString("D");
            await collection.UpsertAsync(accepted, token);
            await Assert.ThrowsAsync<InvalidOperationException>(() => vector.SearchAsync("RRHH",
                new("RRHH", "policy", 2026), metadata, token));
        }
        finally
        {
            // Sólo borramos la colección con nombre aleatorio creada por este test.
            await collection.EnsureCollectionDeletedAsync(CancellationToken.None);
            if (Directory.Exists(indexPath)) Directory.Delete(indexPath, recursive: true);
        }
    }

    // El retrieval y los índices son reales; sólo el embedding remoto se reemplaza por un vector controlado.
    private sealed class ControlledEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(_ => new Embedding<float>(new float[] { 1, 0 }))));
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
