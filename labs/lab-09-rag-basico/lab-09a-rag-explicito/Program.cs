using Microsoft.Extensions.AI;

LabConfiguration configuration;

try
{
    configuration = LabConfiguration.Load();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    Environment.ExitCode = 1;
    return;
}

using IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    EmbeddingGeneratorFactory.Create(configuration);
using IChatClient chatClient = ChatClientFactory.Create(configuration);

const int chunkSize = 500;
const int overlap = 100;
const int topK = 3;

Console.WriteLine("=== RAG BÁSICO - LAB 09a ===");
Console.WriteLine();
Console.WriteLine("=== DOCUMENTO ===");

string documentPath = Path.Combine(
    AppContext.BaseDirectory, "documents", "guia-dotnet-ai.md");
string text = await DocumentLoader.LoadAsync(documentPath);

IReadOnlyList<DocumentChunk> chunks = TextChunker.Split(
    text,
    source: Path.GetFileName(documentPath),
    chunkSize: chunkSize,
    overlap: overlap);

Console.WriteLine($"Archivo: {Path.GetFileName(documentPath)}");
Console.WriteLine($"Caracteres: {text.Length}");
Console.WriteLine($"Chunks: {chunks.Count}");
Console.WriteLine($"Tamaño máximo: {chunkSize}");
Console.WriteLine($"Overlap: {overlap}");
Console.WriteLine();
Console.WriteLine("=== INDEXACIÓN ===");
Console.WriteLine($"Modelo de embeddings: {configuration.EmbeddingModel}");

GeneratedEmbeddings<Embedding<float>> chunkEmbeddings =
    await embeddingGenerator.GenerateAsync(
        chunks.Select(chunk => chunk.Content));

if (chunks.Count != chunkEmbeddings.Count)
{
    throw new InvalidOperationException(
        "La cantidad de embeddings recibidos no coincide con la cantidad de chunks.");
}

InMemoryVectorStore vectorStore = new();

for (int i = 0; i < chunks.Count; i++)
{
    vectorStore.Add(new IndexedChunk(
        chunks[i],
        chunkEmbeddings[i].Vector));
}

Console.WriteLine($"Chunks indexados: {chunkEmbeddings.Count}");
Console.WriteLine();
Console.WriteLine("=== CONSULTA ===");

const string question =
    "¿Qué función cumple IChatClient en Microsoft.Extensions.AI?";
Console.WriteLine(question);

GeneratedEmbeddings<Embedding<float>> questionEmbeddings =
    await embeddingGenerator.GenerateAsync([question]);

if (questionEmbeddings.Count != 1)
{
    throw new InvalidOperationException(
        "Se esperaba un único embedding para la pregunta.");
}

IReadOnlyList<SearchResult> results = vectorStore.Search(
    questionEmbeddings[0].Vector,
    topK);

Console.WriteLine();
Console.WriteLine("=== RECUPERACIÓN ===");
Console.WriteLine($"Top K: {topK}");
foreach (SearchResult result in results)
{
    Console.WriteLine(
        $"Chunk {result.Chunk.Index} - similitud: {result.Similarity:F4}");
}

string context = string.Join("\n\n", results.Select(result =>
    $"[Fuente: {result.Chunk.Source} - Chunk {result.Chunk.Index}]\n{result.Chunk.Content}"));

Console.WriteLine();
Console.WriteLine("=== CONTEXTO RECUPERADO ===");
Console.WriteLine(context);

string prompt = PromptTemplates.Rag(context, question);

Console.WriteLine();
Console.WriteLine("=== GENERACIÓN ===");
Console.WriteLine($"Modelo: {configuration.Model}");

ChatResponse response = await chatClient.GetResponseAsync(prompt);

Console.WriteLine();
Console.WriteLine("=== RESPUESTA ===");
Console.WriteLine(response.Text);
