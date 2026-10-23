using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RagSemanticChunking;

/// <summary>Mediciones de chunking de un documento, guardadas para comparar costes entre estrategias.</summary>
/// <param name="Source">Documento medido.</param>
/// <param name="Characters">Longitud del texto normalizado en unidades UTF-16.</param>
/// <param name="Chunks">Cantidad de fragmentos producidos.</param>
/// <param name="ChunkingMilliseconds">Tiempo de ChunkAsync; excluye embeddings finales y escritura en Qdrant.</param>
/// <param name="AdditionalEmbeddings">Textos vectorizados para decidir cortes, no solicitudes HTTP.</param>
/// <param name="LlmCalls">Decisiones de chat completadas; no cuenta por separado los reintentos del transporte.</param>
/// <param name="AdjacentSimilarities">Valores observados entre unidades vecinas de la estrategia semantic.</param>
public sealed record DocumentMetrics(string Source, int Characters, int Chunks, double ChunkingMilliseconds,
    int AdditionalEmbeddings, int LlmCalls, IReadOnlyList<double> AdjacentSimilarities);

/// <summary>Snapshot local que vincula colección, corpus, configuración, chunks y métricas de una ingesta.</summary>
/// <remarks>
/// Permite comparar ejecuciones compatibles y conocer qué IDs deben eliminarse al reingerir.
/// Generation debe coincidir con los registros recuperados. El manifiesto no contiene la API key.
/// </remarks>
public sealed record IndexManifest(int SchemaVersion, string Strategy, string EmbeddingModel, string ChatModel,
    string ProviderIdentity, int Dimensions, string CorpusHash, string Generation, ChunkingSettings Settings,
    Guid[] Ids, DocumentChunk[] Chunks, DocumentMetrics[] Documents)
{
    /// <summary>Formato legible compartido por manifiestos y reportes generados.</summary>
    public static JsonSerializerOptions JsonOptions { get; } = new() { WriteIndented = true };
    /// <summary>Resuelve el archivo de manifiesto exclusivo de una estrategia validada.</summary>
    public static string PathFor(string artifacts, string strategy) =>
        Path.Combine(artifacts, CommandLine.ValidateStrategy(strategy) + "-manifest.json");
    /// <summary>Calcula la identidad del proveedor y endpoint sin guardar la credencial.</summary>
    /// <remarks>La API key no participa: rotarla no cambia el espacio vectorial del experimento.</remarks>
    public static string Identity(AppConfiguration config) => Convert.ToHexString(SHA256.HashData(
        Encoding.UTF8.GetBytes(config.ProviderName + "\n" + config.Endpoint?.AbsoluteUri)));

    /// <summary>Identifica nombres y contenido normalizado del corpus en el orden recibido.</summary>
    /// <remarks>El loader ordena las rutas para que este hash no dependa del orden del filesystem.</remarks>
    public static string HashCorpus(IReadOnlyList<DocumentText> documents)
    {
        // JSON evita ambigüedad entre nombres, saltos y contenido; se hashea el texto que realmente segmentamos.
        string json = JsonSerializer.Serialize(documents.Select(d => new { d.Source, d.Text }));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }

    /// <summary>Lee y valida un manifiesto existente; no crea una ingesta implícita si falta.</summary>
    public static async Task<IndexManifest> LoadAsync(string artifacts, string strategy, CancellationToken token)
    {
        string path = PathFor(artifacts, strategy);
        if (!File.Exists(path)) throw new InvalidOperationException($"Falta el manifiesto {strategy}. Ejecutá ingest --chunking {strategy}.");
        IndexManifest manifest = JsonSerializer.Deserialize<IndexManifest>(await File.ReadAllTextAsync(path, token), JsonOptions)
            ?? throw new InvalidOperationException("Manifiesto vacío.");
        manifest.Validate(strategy);
        return manifest;
    }

    /// <summary>Comprueba versión, identidad de estrategia y consistencia básica del snapshot.</summary>
    public void Validate(string strategy)
    {
        if (SchemaVersion != ExperimentSettings.SchemaVersion || Strategy != strategy || Dimensions <= 0
            || string.IsNullOrWhiteSpace(EmbeddingModel) || string.IsNullOrWhiteSpace(ChatModel)
            || string.IsNullOrWhiteSpace(ProviderIdentity) || string.IsNullOrWhiteSpace(CorpusHash)
            || string.IsNullOrWhiteSpace(Generation) || Ids is not { Length: > 0 } || Chunks is null
            || Chunks.Length != Ids.Length || Ids.Distinct().Count() != Ids.Length || Documents is not { Length: > 0 } || Settings is null)
            throw new InvalidOperationException("Manifiesto incompatible o inválido. Revisá artifacts y reingestá.");
        Settings.Validate();
    }

    /// <summary>Impide mezclar índices generados con otro proveedor, endpoint o modelos.</summary>
    /// <remarks>Igual dimensión no significa igual espacio vectorial: el modelo también debe coincidir.</remarks>
    public void ValidateConfiguration(AppConfiguration config)
    {
        if (EmbeddingModel != config.EmbeddingModel || ProviderIdentity != Identity(config) || ChatModel != config.Model)
            throw new InvalidOperationException("Proveedor, endpoint o modelos difieren de la ingesta. No mezcles experimentos; reiniciá las colecciones dedicadas.");
    }

    /// <summary>Escribe un archivo temporal y reemplaza el manifiesto al completar la escritura.</summary>
    /// <remarks>Reduce el riesgo de JSON parcial; no crea una transacción entre el archivo y Qdrant.</remarks>
    public async Task SaveAsync(string artifacts, CancellationToken token)
    {
        Directory.CreateDirectory(artifacts);
        string path = PathFor(artifacts, Strategy), temporary = path + ".tmp";
        await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(this, JsonOptions), token);
        File.Move(temporary, path, overwrite: true);
    }
}
