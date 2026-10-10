using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace RagVectorStore;

public sealed class IngestionService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore)
{
    public async Task IngestAsync(string directory, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"No se encontró la carpeta de documentos: {directory}");
        }

        string[] paths = Directory.EnumerateFiles(directory)
            .Where(path => Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (paths.Length == 0)
        {
            throw new InvalidOperationException("No hay documentos .md o .txt para ingestar en data/.");
        }

        Console.WriteLine("=== INGESTA ===");
        Console.WriteLine($"Documentos: {paths.Length} | Chunk size: {RagSettings.ChunkSize} | Overlap: {RagSettings.Overlap}");
        int total = 0;
        int? dimensions = null;
        foreach (string path in paths)
        {
            string source = Path.GetFileName(path);
            string text = await DocumentText.LoadAsync(path, cancellationToken);
            IReadOnlyList<DocumentChunk> chunks = DocumentText.Split(
                text, source, RagSettings.ChunkSize, RagSettings.Overlap);
            Console.WriteLine($"\nDocumento: {source} | Chunks generados: {chunks.Count}");
            if (chunks.Count == 0)
            {
                continue;
            }

            GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(
                embeddingGenerator, chunks.Select(chunk => chunk.Content).ToArray(), cancellationToken);

            dimensions ??= embeddings[0].Vector.Length;
            if (embeddings[0].Vector.Length != dimensions)
            {
                throw new InvalidOperationException("La dimensión cambió durante la ingesta. Revisá el modelo de embeddings.");
            }

            using VectorStoreCollection<Guid, DocumentChunkRecord> collection = vectorStore.GetCollection<Guid, DocumentChunkRecord>(
                RagSettings.CollectionName, DocumentChunkRecord.Definition(dimensions.Value));
            await collection.EnsureCollectionExistsAsync(cancellationToken);

            DocumentChunkRecord[] records = chunks.Select((chunk, index) =>
                DocumentChunkRecord.FromChunk(source, chunk, embeddings[index].Vector)).ToArray();
            await collection.UpsertAsync(records, cancellationToken);
            foreach (DocumentChunkRecord record in records)
            {
                Console.WriteLine($"  Upsert: chunk {record.ChunkIndex} | Id: {record.Id}");
            }

            total += records.Length;
        }

        if (total == 0)
        {
            throw new InvalidOperationException("Los documentos no contienen texto para indexar.");
        }

        Console.WriteLine($"\nDimensión: {dimensions} | Chunks procesados: {total}");
        Console.WriteLine($"Colección actualizada: {RagSettings.CollectionName}");
    }
}
