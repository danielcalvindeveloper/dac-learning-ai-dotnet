using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed record IncidentScenario(string Name, string Report);

internal sealed record TriageResult(
    [property: JsonPropertyName("service")]
    [property: Description("Nombre exacto del servicio: pagos o transferencias.")]
    string Service,
    [property: JsonPropertyName("summary")]
    [property: Description("Resumen breve del problema informado.")]
    string Summary,
    [property: JsonPropertyName("errorCode")]
    [property: Description("Código de error explícito; usa una cadena vacía si no existe.")]
    string ErrorCode);

internal sealed record ServiceStatus(string Status, string? Incident);

internal sealed record DiagnosisInput(
    TriageResult Triage,
    ServiceStatus ServiceStatus,
    string? KnownSolution);

internal sealed record DiagnosisResult(
    [property: JsonPropertyName("finding")]
    [property: Description("Hallazgo breve basado en las observaciones locales.")]
    string Finding,
    [property: JsonPropertyName("recommendedAction")]
    [property: Description("Siguiente paso recomendado, sin inventar datos.")]
    string RecommendedAction);

internal sealed record ResolutionInput(
    string Report,
    TriageResult Triage,
    ServiceStatus ServiceStatus,
    string? KnownSolution,
    DiagnosisResult Diagnosis,
    string? IncidentId);

internal sealed record ResolutionResult(
    [property: JsonPropertyName("response")]
    [property: Description("Respuesta breve y útil para el usuario.")]
    string Response);

internal sealed record SupportCase(
    IncidentScenario Scenario,
    TriageResult? Triage = null,
    ServiceStatus? ServiceStatus = null,
    string? KnownSolution = null,
    DiagnosisResult? Diagnosis = null,
    string? IncidentId = null,
    ResolutionResult? Resolution = null);

internal sealed class SupportData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Dictionary<string, ServiceStatus> serviceStatuses;
    private readonly Dictionary<string, string> knowledgeBase;
    private readonly List<string> incidents = [];

    public SupportData(string dataDirectory)
    {
        serviceStatuses = new(
            JsonSerializer.Deserialize<Dictionary<string, ServiceStatus>>(
                File.ReadAllText(Path.Combine(dataDirectory, "service-status.json")),
                JsonOptions)
                ?? throw new InvalidOperationException(
                    "El archivo de estado de servicios no contiene datos."),
            StringComparer.OrdinalIgnoreCase);

        knowledgeBase = new(
            JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(Path.Combine(dataDirectory, "knowledge-base.json")))
                ?? throw new InvalidOperationException(
                    "La base de conocimiento no contiene datos."),
            StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<IncidentScenario> Scenarios =>
    [
        new("A - Incidente conocido", "No puedo usar transferencias."),
        new("B - Solución conocida", "Pagos devuelve error 503."),
        new("C - Sin explicación conocida", "Pagos devuelve error X91.")
    ];

    public ServiceStatus GetServiceStatus(string service)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);

        if (!serviceStatuses.TryGetValue(service.Trim(), out ServiceStatus? status))
        {
            throw new InvalidOperationException(
                $"No hay información local para el servicio '{service}'.");
        }

        return status;
    }

    public string? FindSolution(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return null;
        }

        knowledgeBase.TryGetValue(errorCode.Trim(), out string? solution);
        return solution;
    }

    public string RegisterIncident(
        string service,
        string description,
        ServiceStatus status,
        string? solution)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(status);

        if (!string.IsNullOrWhiteSpace(status.Incident)
            || !string.IsNullOrWhiteSpace(solution))
        {
            throw new InvalidOperationException(
                "Sólo se registra un incidente sin explicación conocida.");
        }

        string incidentId = $"INC-{1001 + incidents.Count}";
        incidents.Add($"{incidentId} | {service} | {description}");
        return incidentId;
    }
}
