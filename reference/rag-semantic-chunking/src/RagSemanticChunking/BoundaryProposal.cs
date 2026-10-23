using System.Text.Json;
using System.Text.Json.Serialization;

namespace RagSemanticChunking;

/// <summary>Describe un chunk mediante índices de unidades, no posiciones de caracteres.</summary>
/// <param name="StartUnit">Primera unidad incluida, contando desde cero.</param>
/// <param name="EndUnit">Primera unidad excluida: [0, 2) incluye las unidades 0 y 1.</param>
/// <remarks>El modelo propone límites; el contenido se extrae del documento original.</remarks>
public sealed record BoundarySegment(int StartUnit, int EndUnit);
/// <summary>Reúne los segmentos elegidos para validar la partición completa antes de publicarla.</summary>
public sealed record BoundaryProposal(BoundarySegment[] Segments);
/// <summary>Contrato JSON de una decisión del LLM: sólo el próximo límite.</summary>
/// <remarks>La aplicación controla el inicio. Se rechazan campos extra y la ausencia de EndUnit.</remarks>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record BoundarySelection([property: JsonRequired] int EndUnit);

/// <summary>Separa la elección semántica del LLM de las restricciones que garantiza .NET.</summary>
public static class BoundaryProposalValidator
{
    /// <summary>Lee JSON estricto y acepta únicamente una alternativa previamente calculada.</summary>
    /// <param name="json">Respuesta del modelo, sin bloques Markdown ni texto explicativo.</param>
    /// <param name="startUnit">Inicio del chunk actual, ya fijado por la aplicación.</param>
    /// <param name="candidates">Segmentos que respetan tamaños y permiten completar el documento.</param>
    /// <exception cref="InvalidOperationException">El JSON o el límite no cumplen el contrato.</exception>
    public static BoundarySegment Select(string json, int startUnit, IReadOnlyList<BoundarySegment> candidates)
    {
        try
        {
            BoundarySelection selection = JsonSerializer.Deserialize<BoundarySelection>(json)
                ?? throw new InvalidOperationException("La propuesta LLM es null.");
            return candidates.SingleOrDefault(s => s.StartUnit == startUnit && s.EndUnit == selection.EndUnit)
                ?? throw Invalid("fin de unidad fuera de candidatos permitidos");
        }
        catch (JsonException ex)
        { throw new InvalidOperationException("La propuesta LLM no cumple el contrato JSON; no se aplica fallback.", ex); }
    }

    /// <summary>Comprueba cobertura, orden y tamaños, y convierte unidades a rangos de caracteres.</summary>
    /// <returns>Rangos con final exclusivo sobre el texto normalizado, listos para construir chunks.</returns>
    /// <remarks>No repara propuestas inválidas: abortar evita ocultar fallos de la estrategia evaluada.</remarks>
    public static IReadOnlyList<TextRange> Validate(DocumentText document, IReadOnlyList<TextRange> units,
        BoundaryProposal proposal, ChunkingSettings settings)
    {
        settings.Validate();
        if (proposal.Segments is not { Length: > 0 }) throw Invalid("faltan segmentos");
        List<TextRange> ranges = [];
        // Cada segmento comienza donde terminó el anterior: no puede haber huecos ni unidades repetidas.
        int expected = 0;
        foreach (BoundarySegment? segment in proposal.Segments)
        {
            if (segment is null || segment.StartUnit != expected || segment.StartUnit < 0
                || segment.EndUnit <= segment.StartUnit || segment.EndUnit > units.Count)
                throw Invalid("gaps, overlaps, duplicados, orden o límites de unidades");
            // EndUnit es exclusivo: la última unidad incluida está en EndUnit - 1.
            int start = units[segment.StartUnit].Start, end = units[segment.EndUnit - 1].End;
            if (end - start > settings.MaxSize || (end - start < settings.MinSize && document.Text.Length >= settings.MinSize))
                throw Invalid($"{document.Source}, unidades [{segment.StartUnit},{segment.EndUnit}), tamaño {end - start} fuera de {settings.MinSize}..{settings.MaxSize}");
            ranges.Add(new(start, end, "LLM boundary"));
            expected = segment.EndUnit;
        }
        if (expected != units.Count || ranges[0].Start != 0 || ranges[^1].End != document.Text.Length)
            throw Invalid("cobertura incompleta");
        // La validación común verifica que los chunks sean slices exactas, sin texto inventado.
        TextPartition.ValidateCoverage(document, document.Build(ranges), settings.MaxSize, overlap: false);
        return ranges;
    }

    /// <summary>Calcula alternativas que permiten completar toda la partición dentro de min/max.</summary>
    /// <remarks>
    /// No basta con que el próximo chunk sea válido: debe quedar una continuación válida.
    /// Se resuelve desde el final hacia el principio, reutilizando continuaciones ya comprobadas.
    /// </remarks>
    public static IReadOnlyList<BoundarySegment> FeasibleSegments(DocumentText document,
        IReadOnlyList<TextRange> units, ChunkingSettings settings)
    {
        // La aplicación calcula restricciones numéricas; el LLM elige semánticamente entre alternativas.
        // Sólo ofrecemos aristas que permiten llegar al final con tamaños válidos, sin reparaciones posteriores.
        // reachesEnd[i] indica si desde la unidad i se puede completar el documento con chunks válidos.
        // El índice posterior a la última unidad es el caso base: ya no queda texto por dividir.
        bool[] reachesEnd = new bool[units.Count + 1];
        reachesEnd[^1] = true;
        List<BoundarySegment> candidates = [];
        for (int start = units.Count - 1; start >= 0; start--)
            for (int end = start + 1; end <= units.Count; end++)
            {
                int length = units[end - 1].End - units[start].Start;
                // Agregar más unidades sólo aumenta el tamaño; ya no habrá un final válido para este inicio.
                if (length > settings.MaxSize) break;
                // Aceptamos start -> end sólo si desde end existe una continuación válida.
                // Así no ofrecemos un corte que deje un fragmento final demasiado pequeño.
                if (reachesEnd[end] && (length >= settings.MinSize || document.Text.Length < settings.MinSize))
                { reachesEnd[start] = true; candidates.Add(new(start, end)); }
            }
        if (!reachesEnd[0]) throw Invalid("las unidades no permiten ninguna partición dentro de min/max");
        return candidates.OrderBy(c => c.StartUnit).ThenBy(c => c.EndUnit).ToArray();
    }

    private static InvalidOperationException Invalid(string reason) =>
        new($"Propuesta LLM inválida ({reason}). Ingesta abortada sin fallback ni publicación; revisá el modelo o los límites.");
}
