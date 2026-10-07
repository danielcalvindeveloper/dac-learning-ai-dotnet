using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

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

ServiceCollection services = new();

services.AddSingleton<IChatClient>(
    _ => ChatClientFactory.Create(configuration));
services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
    _ => EmbeddingGeneratorFactory.Create(configuration));
services.AddSingleton<InMemoryVectorStore>();
services.AddTransient<IRagService, RagService>();

using ServiceProvider serviceProvider = services.BuildServiceProvider();
IRagService ragService = serviceProvider.GetRequiredService<IRagService>();

string documentPath = Path.Combine(
    AppContext.BaseDirectory, "documents", "guia-dotnet-ai.md");

Console.WriteLine("=== RAG BÁSICO - LAB 09b ===");
Console.WriteLine();
Console.WriteLine($"Documento: {Path.GetFileName(documentPath)}");
Console.WriteLine("Indexando documento...");

await ragService.IndexDocumentAsync(documentPath);

Console.WriteLine("Documento indexado.");
Console.WriteLine();
Console.WriteLine("=== CONSULTA ===");

const string question =
    "¿Qué función cumple IChatClient en Microsoft.Extensions.AI?";
const int topK = 3;
Console.WriteLine(question);

RagResponse response = await ragService.AskAsync(question, topK);

Console.WriteLine();
Console.WriteLine("=== CHUNKS RECUPERADOS ===");
Console.WriteLine($"Top K: {topK}");
foreach (SearchResult source in response.Sources)
{
    Console.WriteLine(
        $"Chunk {source.Chunk.Index} ({source.Chunk.Source}) - similitud: {source.Similarity:F4}");
}

Console.WriteLine();
Console.WriteLine("=== RESPUESTA ===");
Console.WriteLine(response.Answer);
