using Microsoft.Extensions.AI;

LabConfiguration configuration;

try
{
    configuration = LabConfiguration.Load();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    return;
}

using IEmbeddingGenerator<string, Embedding<float>> generator =
    EmbeddingGeneratorFactory.Create(configuration);

const string query =
    "Quiero comprar una notebook para programar en Java";
const string documentA =
    "Laptop para desarrollo de software con 32GB RAM";
const string documentB =
    "Receta de pizza con salsa de tomate y mozzarella";

GeneratedEmbeddings<Embedding<float>> embeddings =
    await generator.GenerateAsync([query, documentA, documentB]);

double similarityA = VectorSimilarity.CosineSimilarity(
    embeddings[0].Vector.Span,
    embeddings[1].Vector.Span);
double similarityB = VectorSimilarity.CosineSimilarity(
    embeddings[0].Vector.Span,
    embeddings[2].Vector.Span);

var ranking = new[]
{
    (Document: "Documento A", Similarity: similarityA),
    (Document: "Documento B", Similarity: similarityB)
}.OrderByDescending(result => result.Similarity).ToArray();

Console.WriteLine("=== EMBEDDINGS ===");
Console.WriteLine();
Console.WriteLine($"Consulta:\n{query}\n");
Console.WriteLine($"Documento A:\n{documentA}\n");
Console.WriteLine($"Documento B:\n{documentB}\n");
Console.WriteLine($"Dimensión del vector: {embeddings[0].Vector.Length}");
Console.WriteLine();
Console.WriteLine($"Similitud consulta / documento A: {similarityA:F4}");
Console.WriteLine($"Similitud consulta / documento B: {similarityB:F4}");
Console.WriteLine();
Console.WriteLine("Documentos ordenados por similitud:");
foreach (var result in ranking)
{
    Console.WriteLine($"{result.Document}: {result.Similarity:F4}");
}

Console.WriteLine();
Console.WriteLine(similarityA == similarityB
    ? "Documento más relacionado: empate"
    : $"Documento más relacionado: {ranking[0].Document}");
