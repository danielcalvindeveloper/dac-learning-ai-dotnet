namespace RagVectorStore;

public static class RagSettings
{
    public const string CollectionName = "dac_rag_documents";
    public const int ChunkSize = 500;
    public const int Overlap = 100;
    public const int TopK = 3;

    // Umbral experimental de score Qdrant/Cosine para este corpus; requiere recalibración al cambiar el modelo.
    public const double MinimumScore = 0.65;
}
