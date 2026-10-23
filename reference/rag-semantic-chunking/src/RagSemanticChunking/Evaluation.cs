using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace RagSemanticChunking;

/// <summary>Pregunta identificada y documentos relevantes esperados, definidos antes de medir resultados.</summary>
public sealed record EvaluationQuery(string Id, string Question, string[] RelevantDocuments);
/// <summary>Conjunto versionado de preguntas que permite repetir la comparación sobre el mismo criterio.</summary>
public sealed record QuerySet(int Version, EvaluationQuery[] Queries)
{
    /// <summary>Rechaza IDs duplicados y relevancia esperada que mencione archivos ajenos al corpus.</summary>
    public static async Task<QuerySet> LoadAsync(string path, IReadOnlyList<DocumentText> corpus, CancellationToken token)
    {
        QuerySet set = JsonSerializer.Deserialize<QuerySet>(await File.ReadAllTextAsync(path, token))
            ?? throw new InvalidOperationException("Queries vacías.");
        string[] sources = corpus.Select(d => d.Source).ToArray();
        if (set.Version != 1 || set.Queries is not { Length: > 0 }
            || set.Queries.Select(q => q.Id).Distinct(StringComparer.Ordinal).Count() != set.Queries.Length
            || set.Queries.Any(q => string.IsNullOrWhiteSpace(q.Id) || string.IsNullOrWhiteSpace(q.Question)
                || q.RelevantDocuments is not { Length: > 0 }
                || q.RelevantDocuments.Distinct(StringComparer.Ordinal).Count() != q.RelevantDocuments.Length
                || q.RelevantDocuments.Any(d => !sources.Contains(d, StringComparer.Ordinal))))
            throw new InvalidOperationException("Queries inválidas: identificadores únicos y documentos relevantes existentes requeridos.");
        return set;
    }
}

/// <summary>Traza compacta de un resultado recuperado; conserva identidad y score sin duplicar todo su texto.</summary>
public sealed record EvaluationHit(string Source, int ChunkIndex, string Section, double Score);
/// <summary>Medición de una pregunta para una estrategia, con el ranking utilizado para calcularla.</summary>
public sealed record QueryMeasurement(string QueryId, string Strategy, double Recall, double ReciprocalRank,
    IReadOnlyList<EvaluationHit> Hits);
/// <summary>Resumen de tamaños, límites, costes de corte y métricas de retrieval por estrategia.</summary>
public sealed record StrategyMeasurement(string Strategy, int Chunks, double AverageSize, int MinimumSize,
    int MaximumSize, int Boundaries, int AdditionalEmbeddings, int LlmCalls, double ChunkingMilliseconds,
    double MeanRecall, double Mrr);
/// <summary>Reporte reproducible: registra preguntas, parámetros e identidad del corpus junto a las mediciones.</summary>
public sealed record ComparisonReport(int QueryVersion, int TopK, string CorpusHash, string EmbeddingModel,
    string ChatModel, ChunkingSettings Settings, IReadOnlyList<EvaluationQuery> Queries,
    IReadOnlyList<StrategyMeasurement> Strategies, IReadOnlyList<QueryMeasurement> Measurements);

/// <summary>Cálculos deterministas que evalúan recuperación de documentos, sin juzgar respuestas del LLM.</summary>
public static class Evaluation
{
    /// <summary>Calcula qué fracción de los documentos relevantes aparece entre los chunks recuperados.</summary>
    /// <remarks>Tres chunks del mismo documento cuentan como un documento: repetir evidencia no aumenta Recall.</remarks>
    public static double Recall(IReadOnlyList<string> relevant, IReadOnlyList<string> retrieved)
    {
        string[] expected = relevant.Distinct(StringComparer.Ordinal).ToArray();
        if (expected.Length == 0) throw new ArgumentException("Recall necesita relevancia esperada.");
        // Convertir a double evita división entera: encontrar 1 de 2 documentos debe dar 0.5, no 0.
        return (double)expected.Intersect(retrieved, StringComparer.Ordinal).Count() / expected.Length;
    }

    /// <summary>Devuelve 1 / posición del primer chunk de un documento relevante, o cero si no aparece.</summary>
    /// <remarks>Las posiciones comienzan en 1. Esta métrica premia encontrar pronto, no cubrir toda la evidencia.</remarks>
    public static double ReciprocalRank(IReadOnlyList<string> relevant, IReadOnlyList<string> retrieved)
    {
        for (int i = 0; i < retrieved.Count; i++) if (relevant.Contains(retrieved[i], StringComparer.Ordinal)) return 1d / (i + 1);
        return 0;
    }

    /// <summary>Exige las cuatro estrategias y el mismo corpus, configuración, parámetros y dimensión.</summary>
    /// <remarks>Comparar índices incompatibles atribuiría al chunking diferencias causadas por otras variables.</remarks>
    public static void ValidateComparable(IReadOnlyList<IndexManifest> manifests, AppConfiguration config, string corpusHash)
    {
        if (!manifests.Select(m => m.Strategy).Order(StringComparer.Ordinal)
            .SequenceEqual(CommandLine.Strategies.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("Compare requiere las cuatro estrategias, sin duplicados.");
        foreach (IndexManifest manifest in manifests)
        {
            manifest.ValidateConfiguration(config);
            if (manifest.CorpusHash != corpusHash || manifest.Settings != manifests[0].Settings
                || manifest.Dimensions != manifests[0].Dimensions)
                throw new InvalidOperationException("Las ingestas no son comparables: corpus, parámetros o dimensión diferentes.");
        }
    }

    /// <summary>Agrega métricas de ingesta y promedia Recall y reciprocal rank sobre las mismas preguntas.</summary>
    /// <remarks>MRR es la media de reciprocal rank; el tiempo de chunking excluye persistencia y embeddings finales.</remarks>
    public static ComparisonReport Summarize(QuerySet queries, IReadOnlyList<IndexManifest> manifests,
        IReadOnlyList<QueryMeasurement> measurements)
    {
        List<StrategyMeasurement> summaries = [];
        foreach (IndexManifest m in manifests)
        {
            QueryMeasurement[] results = measurements.Where(r => r.Strategy == m.Strategy).ToArray();
            if (results.Length != queries.Queries.Length || !results.Select(r => r.QueryId).Order(StringComparer.Ordinal)
                .SequenceEqual(queries.Queries.Select(q => q.Id).Order(StringComparer.Ordinal)))
                throw new ArgumentException("Faltan mediciones o hay queries duplicadas.");
            // Boundaries cuenta cortes internos por documento: N chunks implican N - 1 límites.
            int[] lengths = m.Chunks.Select(c => c.Content.Length).ToArray();
            summaries.Add(new(m.Strategy, lengths.Length, lengths.Average(), lengths.Min(), lengths.Max(),
                m.Documents.Sum(d => d.Chunks - 1), m.Documents.Sum(d => d.AdditionalEmbeddings),
                m.Documents.Sum(d => d.LlmCalls), m.Documents.Sum(d => d.ChunkingMilliseconds),
                results.Average(r => r.Recall), results.Average(r => r.ReciprocalRank)));
        }
        IndexManifest first = manifests[0];
        return new(queries.Version, ExperimentSettings.TopK, first.CorpusHash, first.EmbeddingModel, first.ChatModel,
            first.Settings, queries.Queries, summaries, measurements);
    }

    /// <summary>Presenta el reporte para lectura humana con formato numérico independiente de la cultura.</summary>
    /// <remarks>Incluye límites de interpretación: retrieval no equivale a exactitud de una respuesta generada.</remarks>
    public static string Markdown(ComparisonReport report)
    {
        StringBuilder text = new($"# Comparación de chunking\n\nQueries v{report.QueryVersion}, topK={report.TopK} chunks. Recall a nivel documento. MRR sobre el primer chunk de un documento relevante.\n\n");
        text.AppendLine($"Embeddings: `{report.EmbeddingModel}`. Chat configurado: `{report.ChatModel}` (compare no invoca chat). Corpus SHA256: `{report.CorpusHash}`.\n");
        text.AppendLine("| Estrategia | Chunks | Media | Min/max | Boundaries | Embeddings extra | LLM calls | Chunking ms | Recall@K | MRR |\n|---|---:|---:|---|---:|---:|---:|---:|---:|---:|");
        foreach (StrategyMeasurement s in report.Strategies)
            text.AppendLine(FormattableString.Invariant($"| {s.Strategy} | {s.Chunks} | {s.AverageSize:F1} | {s.MinimumSize}/{s.MaximumSize} | {s.Boundaries} | {s.AdditionalEmbeddings} | {s.LlmCalls} | {s.ChunkingMilliseconds:F0} | {s.MeanRecall:F3} | {s.Mrr:F3} |"));
        foreach (EvaluationQuery q in report.Queries)
        {
            text.AppendLine($"\n## {q.Id}: {q.Question}\n\nEsperados: {string.Join(", ", q.RelevantDocuments)}.\n");
            foreach (QueryMeasurement m in report.Measurements.Where(m => m.QueryId == q.Id))
                text.AppendLine(FormattableString.Invariant($"- {m.Strategy}: Recall@{report.TopK}={m.Recall:F3}, RR={m.ReciprocalRank:F3}; hits: {string.Join(", ", m.Hits.Select(h => $"{h.Source}#{h.ChunkIndex}"))}."));
        }
        text.AppendLine("\nNo mide exactitud de respuestas, groundedness ni suficiencia de cada fragmento. Corpus pequeño y preguntas conocidas: resultados exploratorios, sin ganador universal.");
        return text.ToString();
    }
}

/// <summary>Ejecuta preguntas sobre índices existentes, reutilizando un único embedding por pregunta.</summary>
/// <remarks>No consulta chat ni recalcula chunks; mide retrieval sobre ingestas previamente preparadas.</remarks>
public sealed class ComparisonService(IEmbeddingGenerator<string, Embedding<float>> generator, VectorSearchService search)
{
    /// <summary>Valida comparabilidad, mide rankings y guarda reportes JSON y Markdown.</summary>
    public async Task<ComparisonReport> CompareAsync(QuerySet queries, IReadOnlyList<IndexManifest> manifests,
        AppConfiguration config, string corpusHash, string artifacts, CancellationToken token)
    {
        Evaluation.ValidateComparable(manifests, config, corpusHash);
        List<QueryMeasurement> measurements = [];
        foreach (EvaluationQuery query in queries.Queries)
        {
            // Exactamente el MISMO vector de consulta llega a las cuatro colecciones.
            GeneratedEmbeddings<Embedding<float>> embeddings = await EmbeddingGeneration.GenerateAsync(generator, [query.Question], token);
            foreach (IndexManifest manifest in manifests)
            {
                IReadOnlyList<SearchHit> hits = await search.SearchAsync(embeddings[0].Vector, manifest, token);
                // Conservamos el orden y duplicados del ranking para RR; Recall deduplica documentos por su cuenta.
                string[] sources = hits.Select(h => h.Record.Source).ToArray();
                measurements.Add(new(query.Id, manifest.Strategy, Evaluation.Recall(query.RelevantDocuments, sources),
                    Evaluation.ReciprocalRank(query.RelevantDocuments, sources),
                    hits.Select(h => new EvaluationHit(h.Record.Source, h.Record.ChunkIndex, h.Record.Section, h.Score)).ToArray()));
            }
        }
        ComparisonReport report = Evaluation.Summarize(queries, manifests, measurements);
        Directory.CreateDirectory(artifacts);
        await File.WriteAllTextAsync(Path.Combine(artifacts, "comparison.json"), JsonSerializer.Serialize(report, IndexManifest.JsonOptions), token);
        await File.WriteAllTextAsync(Path.Combine(artifacts, "comparison.md"), Evaluation.Markdown(report), token);
        return report;
    }
}
