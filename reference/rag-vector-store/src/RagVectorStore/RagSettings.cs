namespace RagVectorStore;

public static class RagSettings
{
    // Cambiar modelo o chunking exige revisar el índice existente; el nombre no versiona esos cambios.
    public const string CollectionName = "dac_rag_documents";
    // Unidades UTF-16. La ventana avanza ChunkSize - Overlap; no usamos tokenización.
    public const int ChunkSize = 500;
    public const int Overlap = 100;
    // Cantidad máxima de candidatos, antes del filtro de MinimumScore.
    public const int TopK = 3;

    // Umbral experimental de score Qdrant/Cosine para este corpus; requiere recalibración al cambiar el modelo.
    public const double MinimumScore = 0.65;
}
