using Microsoft.Extensions.AI;

namespace RagSemanticChunking;

/// <summary>Busca posibles cambios de tema comparando embeddings de unidades contiguas.</summary>
/// <remarks>Estos vectores sirven para cortar; después la ingesta vectoriza los chunks finales para retrieval.</remarks>
public sealed class EmbeddingSemanticChunkingStrategy(ChunkingSettings settings,
    IEmbeddingGenerator<string, Embedding<float>> generator) : IChunkingStrategy
{
    /// <inheritdoc/>
    public string Name => "semantic";
    /// <inheritdoc/>
    public async Task<ChunkingResult> ChunkAsync(DocumentText document, CancellationToken cancellationToken)
    {
        settings.Validate();
        IReadOnlyList<TextRange> units = document.Units(settings.UnitMaxSize);
        if (units.Count == 0) return new([]);
        GeneratedEmbeddings<Embedding<float>> vectors = await EmbeddingGeneration.GenerateAsync(generator,
            units.Select(u => document.Text[u.Start..u.End]).ToArray(), cancellationToken);
        // Comparamos i con i + 1, no todos contra todos. Una caída sugiere un cambio local,
        // pero no demuestra que exista un límite semántico correcto.
        double[] similarities = Enumerable.Range(0, units.Count - 1)
            .Select(i => Cosine(vectors[i].Vector.Span, vectors[i + 1].Vector.Span)).ToArray();
        return new(document.Build(Partition(document, units, similarities, settings)), units.Count, 0, similarities);
    }

    /// <summary>Convierte similitudes adyacentes en cortes respetando tamaños y cobertura.</summary>
    /// <param name="document">Texto normalizado que se dividirá sin reescribirlo.</param>
    /// <param name="units">Unidades contiguas de ese mismo texto.</param>
    /// <param name="similarities">N - 1 valores: la posición i compara las unidades i e i + 1.</param>
    /// <param name="settings">Tamaños y umbral común del experimento.</param>
    /// <remarks>Separar esta lógica de la generación permite probar los cortes sin red ni modelos.</remarks>
    public static IReadOnlyList<TextRange> Partition(DocumentText document, IReadOnlyList<TextRange> units,
        IReadOnlyList<double> similarities, ChunkingSettings settings)
    {
        settings.Validate();
        if (similarities.Count != Math.Max(0, units.Count - 1) || similarities.Any(s => !double.IsFinite(s) || s is < -1.000001 or > 1.000001))
            throw new ArgumentException("La secuencia de similitudes no corresponde a las unidades.");
        List<TextRange> chunks = [];
        int start = 0;
        while (start < document.Text.Length)
        {
            int remaining = document.Text.Length - start;
            if (remaining <= settings.MaxSize) // Aun en la cola pueden existir cambios de tema.
            {
                int boundary = SemanticBoundary(start, document.Text.Length);
                if (boundary > start) { chunks.Add(new(start, boundary, "cosine below threshold")); start = boundary; continue; }
                chunks.Add(new(start, document.Text.Length, "document end")); break;
            }
            int limit = start + settings.MaxSize;
            int semantic = SemanticBoundary(start, limit);
            int cut;
            string reason;
            if (semantic > start) { cut = semantic; reason = "cosine below threshold"; }
            else
            {
                // Sin un cambio de tema válido elegimos el último final de unidad que cabe.
                // Si ninguno sirve, cortamos por caracteres y reservamos MinSize para la cola.
                cut = units.Select(u => u.End).Where(end => Valid(end, limit)).DefaultIfEmpty(0).Max();
                if (cut == 0) cut = TextPartition.SafeCut(document.Text,
                    Math.Min(limit, document.Text.Length - settings.MinSize));
                reason = "max size";
            }
            chunks.Add(new(start, cut, reason));
            start = cut;

            // El mínimo se exige al fragmento actual y al texto restante; un corte no debe dejar una cola inválida.
            bool Valid(int end, int max) => end - start >= settings.MinSize && end <= max
                && document.Text.Length - end >= settings.MinSize;
        }
        return chunks;

        // Elegimos la primera caída válida desde el inicio actual. El umbral es una heurística,
        // no una probabilidad de cambio de tema ni una medida de relevancia de la consulta.
        int SemanticBoundary(int current, int limit)
        {
            for (int i = 0; i < similarities.Count; i++)
            {
                int end = units[i].End;
                if (end - current >= settings.MinSize && end <= limit
                    && document.Text.Length - end >= settings.MinSize && similarities[i] < settings.SimilarityThreshold)
                    return end;
            }
            return 0;
        }
    }

    /// <summary>Mide la orientación relativa de dos vectores mediante producto punto y magnitudes.</summary>
    /// <returns>Coseno entre -1 y 1; mayor valor indica mayor alineación entre vectores.</returns>
    /// <exception cref="ArgumentException">Los vectores están vacíos, difieren en dimensión, no son finitos o tienen magnitud cero.</exception>
    public static double Cosine(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length == 0 || a.Length != b.Length) throw new ArgumentException("Vectores vacíos o de distinta dimensión.");
        // dot acumula a[i] * b[i]. Las magnitudes acumulan cuadrados; la raíz se aplica al final.
        // Calculamos en double para reducir errores de acumulación aunque el modelo devuelva float.
        double dot = 0, magnitudeA = 0, magnitudeB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            if (!float.IsFinite(a[i]) || !float.IsFinite(b[i])) throw new ArgumentException("Vector no finito.");
            dot += (double)a[i] * b[i]; magnitudeA += (double)a[i] * a[i]; magnitudeB += (double)b[i] * b[i];
        }
        if (magnitudeA == 0 || magnitudeB == 0) throw new ArgumentException("Vector de magnitud cero.");
        // Clamp corrige pequeños desbordes numéricos fuera de [-1, 1] al dividir por las magnitudes.
        return Math.Clamp(dot / Math.Sqrt(magnitudeA * magnitudeB), -1, 1);
    }
}
