namespace RagHybridSearch;

public static class SearchSettings
{
    public const string CollectionName = "dac_hybrid_documents";
    public const int ChunkSize = 500;
    public const int Overlap = 100;
    // Recuperamos más candidatos que contexto final para que la fusión tenga alternativas.
    public const int TopKVector = 6;
    public const int TopKBm25 = 6;
    // Dos fragmentos acotan el contexto y permiten observar si recuperamos ambas evidencias del caso C.
    public const int TopKFinal = 2;
    public const int RrfConstant = 60;
    public const double Bm25K1 = 1.2;
    public const double Bm25B = 0.75;
    public const int MaximumQueryTerms = 64;
    // Cosine únicamente: no aplicar este umbral a BM25 ni a RRF.
    public const double MinimumVectorScore = 0.60;
}
