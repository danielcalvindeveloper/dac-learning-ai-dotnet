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
// Valor experimental para este corpus; no equivale a un porcentaje.
const double minimumSimilarity = 0.70;

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

Console.WriteLine("=== RETRIEVAL - RELEVANCIA Y THRESHOLD ===");
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
Console.WriteLine($"Top K: {topK}");
Console.WriteLine($"Threshold mínimo experimental: {minimumSimilarity:F4}");

(string Title, string Question)[] queries =
[
    ("RELACIONADA", "¿Cómo se relacionan los embeddings con un sistema RAG?"),
    ("NO RELACIONADA", "¿Cuáles son las principales características de la fotosíntesis en las plantas?")
];

foreach ((string title, string question) in queries)
{
    Console.WriteLine();
    Console.WriteLine($"=== CONSULTA - {title} ===");
    Console.WriteLine(question);

    GeneratedEmbeddings<Embedding<float>> questionEmbeddings =
        await embeddingGenerator.GenerateAsync([question]);

    if (questionEmbeddings.Count != 1)
    {
        throw new InvalidOperationException(
            "Se esperaba un único embedding para la pregunta.");
    }

    Embedding<float> questionEmbedding = questionEmbeddings[0];
    IReadOnlyList<SearchResult> candidates = vectorStore.Search(
        questionEmbedding.Vector,
        topK);

    Console.WriteLine();
    Console.WriteLine("=== CANDIDATOS TOP K ===");
    foreach (SearchResult candidate in candidates)
    {
        Console.WriteLine(
            $"{candidate.Chunk.Source} - Chunk {candidate.Chunk.Index} - similitud: {candidate.Similarity:F4}");
    }

    IReadOnlyList<SearchResult> relevantResults = candidates
        .Where(result => result.Similarity >= minimumSimilarity)
        .ToArray();

    Console.WriteLine();
    Console.WriteLine("=== RESULTADOS RELEVANTES ===");
    Console.WriteLine($"Threshold mínimo experimental: {minimumSimilarity:F4}");
    Console.WriteLine(
        $"{relevantResults.Count} de {candidates.Count} candidatos alcanzan el threshold.");

    if (relevantResults.Count == 0)
    {
        Console.WriteLine("No se encontró contexto suficientemente relevante para responder la pregunta.");
        Console.WriteLine("No se realizará una llamada al modelo generativo.");
        continue;
    }

    string context = string.Join("\n\n", relevantResults.Select(result =>
        $"[Fuente: {result.Chunk.Source} - Chunk {result.Chunk.Index}]\n{result.Chunk.Content}"));

    Console.WriteLine();
    Console.WriteLine("=== CONTEXTO ACEPTADO ===");
    Console.WriteLine(context);

    string prompt = PromptTemplates.Rag(context, question);

    Console.WriteLine();
    Console.WriteLine("=== GENERACIÓN ===");
    Console.WriteLine($"Modelo: {configuration.Model}");

    ChatResponse response = await chatClient.GetResponseAsync(prompt);

    Console.WriteLine();
    Console.WriteLine("=== RESPUESTA ===");
    Console.WriteLine(response.Text);
    Console.WriteLine();
    Console.WriteLine("=== FUENTES UTILIZADAS ===");
    foreach (SearchResult source in relevantResults)
    {
        Console.WriteLine(
            $"- {source.Chunk.Source} - Chunk {source.Chunk.Index} - similitud: {source.Similarity:F4}");
    }
}
