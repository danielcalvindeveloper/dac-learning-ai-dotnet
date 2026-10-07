public sealed record IndexedChunk(
    DocumentChunk Chunk,
    ReadOnlyMemory<float> Vector);
