const int chunkSize = 500;
const int overlap = 100;

string documentPath = Path.Combine(
    AppContext.BaseDirectory, "documents", "introduccion-ia.md");

string text = await DocumentLoader.LoadAsync(documentPath);

IReadOnlyList<DocumentChunk> chunks = TextChunker.Split(
    text,
    source: Path.GetFileName(documentPath),
    chunkSize: chunkSize,
    overlap: overlap);

Console.WriteLine("=== DOCUMENT LOADING ===");
Console.WriteLine();
Console.WriteLine($"Archivo: {Path.GetFileName(documentPath)}");
Console.WriteLine($"Caracteres leídos: {text.Length}");
Console.WriteLine();
Console.WriteLine("=== CHUNKING ===");
Console.WriteLine();
Console.WriteLine($"Tamaño máximo: {chunkSize}");
Console.WriteLine($"Overlap: {overlap}");
Console.WriteLine($"Chunks generados: {chunks.Count}");

foreach (DocumentChunk chunk in chunks)
{
    Console.WriteLine();
    Console.WriteLine($"--- Chunk {chunk.Index} ---");
    Console.WriteLine($"Source: {chunk.Source}");
    Console.WriteLine();
    Console.WriteLine(chunk.Content);
}
