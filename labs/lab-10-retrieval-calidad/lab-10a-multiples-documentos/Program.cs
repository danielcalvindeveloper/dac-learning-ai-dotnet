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

string documentsPath = Path.Combine(AppContext.BaseDirectory, "documents");
string[] documentPaths = Directory.GetFiles(documentsPath, "*.md");
Array.Sort(documentPaths, StringComparer.Ordinal);

if (documentPaths.Length == 0)
{
    throw new InvalidOperationException(
        "No se encontraron documentos Markdown en la carpeta documents.");
}

InMemoryVectorStore vectorStore = new();
int totalChunks = 0;

Console.WriteLine("=== RETRIEVAL - MÚLTIPLES DOCUMENTOS ===");
Console.WriteLine();
Console.WriteLine("=== CORPUS ===");
Console.WriteLine($"Documentos encontrados: {documentPaths.Length}");
Console.WriteLine($"Tamaño máximo: {chunkSize}");
Console.WriteLine($"Overlap: {overlap}");
Console.WriteLine($"Modelo de embeddings: {configuration.EmbeddingModel}");

foreach (string documentPath in documentPaths)
{
    string source = Path.GetFileName(documentPath);
    string text = await DocumentLoader.LoadAsync(documentPath);

    IReadOnlyList<DocumentChunk> chunks = TextChunker.Split(
        text,
        source: source,
        chunkSize: chunkSize,
        overlap: overlap);

    Console.WriteLine();
    Console.WriteLine(source);
    Console.WriteLine($"  Caracteres: {text.Length}");
    Console.WriteLine($"  Chunks: {chunks.Count}");

    GeneratedEmbeddings<Embedding<float>> chunkEmbeddings =
        await embeddingGenerator.GenerateAsync(
            chunks.Select(chunk => chunk.Content));

    if (chunks.Count != chunkEmbeddings.Count)
    {
        throw new InvalidOperationException(
            "La cantidad de embeddings recibidos no coincide con la cantidad de chunks.");
    }

    for (int i = 0; i < chunks.Count; i++)
    {
        vectorStore.Add(new IndexedChunk(
            chunks[i],
            chunkEmbeddings[i].Vector));
    }

    totalChunks += chunks.Count;
}

Console.WriteLine();
Console.WriteLine("=== INDEXACIÓN ===");
Console.WriteLine($"Chunks totales indexados: {totalChunks}");
Console.WriteLine();
Console.WriteLine("=== CONSULTA ===");

const string question =
    "¿Cómo se relacionan los embeddings con un sistema RAG?";
Console.WriteLine(question);

GeneratedEmbeddings<Embedding<float>> questionEmbeddings =
    await embeddingGenerator.GenerateAsync([question]);

if (questionEmbeddings.Count != 1)
{
    throw new InvalidOperationException(
        "Se esperaba un único embedding para la pregunta.");
}

Embedding<float> questionEmbedding = questionEmbeddings[0];
IReadOnlyList<SearchResult> results = vectorStore.Search(
    questionEmbedding.Vector,
    topK);

Console.WriteLine();
Console.WriteLine("=== RECUPERACIÓN ===");
Console.WriteLine($"Top K: {topK}");
foreach (SearchResult result in results)
{
    Console.WriteLine(
        $"{result.Chunk.Source} - Chunk {result.Chunk.Index} - similitud: {result.Similarity:F4}");
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
