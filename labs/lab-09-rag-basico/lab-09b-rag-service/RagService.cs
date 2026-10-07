using Microsoft.Extensions.AI;

public sealed class RagService : IRagService
{
    private const int ChunkSize = 500;
    private const int Overlap = 100;

    private readonly IChatClient _chatClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly InMemoryVectorStore _vectorStore;

    public RagService(
        IChatClient chatClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        InMemoryVectorStore vectorStore)
    {
        _chatClient = chatClient;
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
    }

    public async Task IndexDocumentAsync(string documentPath)
    {
        string text = await DocumentLoader.LoadAsync(documentPath);

        IReadOnlyList<DocumentChunk> chunks = TextChunker.Split(
            text,
            source: Path.GetFileName(documentPath),
            chunkSize: ChunkSize,
            overlap: Overlap);

        GeneratedEmbeddings<Embedding<float>> chunkEmbeddings =
            await _embeddingGenerator.GenerateAsync(
                chunks.Select(chunk => chunk.Content));

        if (chunks.Count != chunkEmbeddings.Count)
        {
            throw new InvalidOperationException(
                "La cantidad de embeddings recibidos no coincide con la cantidad de chunks.");
        }

        for (int i = 0; i < chunks.Count; i++)
        {
            _vectorStore.Add(new IndexedChunk(
                chunks[i],
                chunkEmbeddings[i].Vector));
        }
    }

    public async Task<RagResponse> AskAsync(
        string question,
        int topK = 3)
    {
        GeneratedEmbeddings<Embedding<float>> questionEmbeddings =
            await _embeddingGenerator.GenerateAsync([question]);

        if (questionEmbeddings.Count != 1)
        {
            throw new InvalidOperationException(
                "Se esperaba un único embedding para la pregunta.");
        }

        IReadOnlyList<SearchResult> results = _vectorStore.Search(
            questionEmbeddings[0].Vector,
            topK);

        string context = string.Join("\n\n", results.Select(result =>
            $"[Fuente: {result.Chunk.Source} - Chunk {result.Chunk.Index}]\n{result.Chunk.Content}"));

        string prompt = PromptTemplates.Rag(context, question);

        ChatResponse response = await _chatClient.GetResponseAsync(prompt);

        return new RagResponse(response.Text, results);
    }
}
