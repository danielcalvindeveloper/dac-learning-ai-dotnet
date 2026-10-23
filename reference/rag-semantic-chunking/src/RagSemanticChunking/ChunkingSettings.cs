namespace RagSemanticChunking;

// Parámetros del experimento, versionados junto al corpus y comunes a cada ejecución.
/// <summary>Parámetros comunes para comparar estrategias sobre el mismo experimento.</summary>
/// <param name="MaxSize">Máximo de unidades UTF-16 por chunk, no tokens del modelo.</param>
/// <param name="MinSize">Mínimo para las particiones semántica y LLM, salvo un documento completo menor.</param>
/// <param name="Overlap">Solapamiento del baseline fixed; las otras estrategias no lo utilizan.</param>
/// <param name="UnitMaxSize">Máximo de cada unidad previa al agrupamiento semántico o LLM.</param>
/// <param name="SimilarityThreshold">Coseno por debajo del cual se considera un posible cambio de tema.</param>
/// <param name="LlmMaxDocumentSize">Límite de documento para acotar el contexto enviado al LLM.</param>
/// <remarks>Fixed y structure pueden producir chunks menores que MinSize al respetar sus límites naturales.</remarks>
public sealed record ChunkingSettings(int MaxSize = 800, int MinSize = 120, int Overlap = 100,
    int UnitMaxSize = 400, double SimilarityThreshold = 0.72, int LlmMaxDocumentSize = 20000)
{
    /// <summary>Rechaza combinaciones que impiden avanzar o reservar un fragmento final válido.</summary>
    public void Validate()
    {
        // Reservar al menos dos mínimos dentro del máximo permite partir sin dejar una cola diminuta.
        // Acotar UnitMaxSize deja espacio para agrupar unidades en las estrategias semánticas.
        if (MinSize <= 0 || MaxSize < 2 * MinSize || Overlap < 0 || Overlap >= MinSize
            || UnitMaxSize < MinSize || UnitMaxSize > MaxSize - MinSize
            || !double.IsFinite(SimilarityThreshold) || SimilarityThreshold is < -1 or > 1
            || LlmMaxDocumentSize < MaxSize)
            throw new ArgumentException("Parámetros inválidos: min > 0, max >= 2*min, 0 <= overlap < min, min <= unidad <= max-min, coseno entre -1 y 1.");
    }
}

/// <summary>Convenciones compartidas de búsqueda y persistencia; no son opciones diferentes por estrategia.</summary>
public static class ExperimentSettings
{
    /// <summary>Cantidad de chunks recuperados; varios pueden provenir del mismo documento.</summary>
    public const int TopK = 3;
    /// <summary>Versión del formato de manifiesto aceptado por esta aplicación.</summary>
    public const int SchemaVersion = 1;
    /// <summary>Prefijo que separa estas colecciones de las utilizadas por otras referencias.</summary>
    public const string CollectionPrefix = "dac_chunking_";
    /// <summary>Construye el nombre de una colección para una estrategia conocida.</summary>
    /// <remarks>Los tests de integración suministran otro prefijo para aislar sus datos.</remarks>
    public static string Collection(string strategy, string prefix = CollectionPrefix) => prefix + CommandLine.ValidateStrategy(strategy);
}
