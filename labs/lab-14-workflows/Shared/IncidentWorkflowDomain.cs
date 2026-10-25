using System.Text.Json;

// The report is structured already so this lab can focus on workflow control, not text extraction.
internal sealed record IncidentRequest(string Service, string Description, string? ErrorCode);

internal sealed record IncidentScenario(string Name, IncidentRequest Request);

// Both sublabs pass the same state between steps; only the way they choose the next step differs.
internal sealed class IncidentWorkflowState(IncidentRequest request)
{
    public IncidentRequest Request { get; } = request;
    public ServiceStatus? ServiceStatus { get; set; }
    public string? Solution { get; set; }
    public string? IncidentId { get; set; }
    public string? Response { get; set; }
    public List<string> ExecutedSteps { get; } = [];

    public bool HasKnownIncident => !string.IsNullOrWhiteSpace(ServiceStatus?.Incident);
}

internal sealed record ServiceStatus(string Status, string? Incident);

// Domain operations shared by 14a and 14b. Workflow transitions stay in each Program.cs.
internal sealed class IncidentWorkflowData
{
    private readonly Dictionary<string, ServiceStatus> serviceStatuses;
    private readonly Dictionary<string, string> knowledgeBase;
    private readonly List<string> incidents = [];

    public IncidentWorkflowData(string dataDirectory)
    {
        serviceStatuses = new(
            JsonSerializer.Deserialize<Dictionary<string, ServiceStatus>>(
                File.ReadAllText(Path.Combine(dataDirectory, "service-status.json")),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("El estado de servicios no contiene datos."),
            StringComparer.OrdinalIgnoreCase);

        knowledgeBase = new(
            JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(Path.Combine(dataDirectory, "knowledge-base.json")))
                ?? throw new InvalidOperationException("La base de conocimiento no contiene datos."),
            StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<IncidentScenario> Scenarios =>
    [
        new(
            "A - Incidente conocido",
            new("transferencias", "No puedo usar transferencias.", null)),
        new(
            "B - Solución conocida",
            new("pagos", "Pagos devuelve error 503.", "503")),
        new(
            "C - Sin solución conocida",
            new("pagos", "Pagos devuelve error X91.", "X91"))
    ];

    public IncidentWorkflowState Validate(IncidentWorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(state.Request.Service);
        ArgumentException.ThrowIfNullOrWhiteSpace(state.Request.Description);
        state.ExecutedSteps.Add("Validar reporte");
        return state;
    }

    public IncidentWorkflowState QueryServiceStatus(IncidentWorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!serviceStatuses.TryGetValue(state.Request.Service.Trim(), out ServiceStatus? status))
        {
            throw new InvalidOperationException($"No hay información para el servicio {state.Request.Service}.");
        }

        state.ServiceStatus = status;
        state.ExecutedSteps.Add("Consultar estado");
        return state;
    }

    public IncidentWorkflowState SearchSolution(IncidentWorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(state.Request.ErrorCode);

        knowledgeBase.TryGetValue(state.Request.ErrorCode.Trim(), out string? solution);
        state.Solution = solution;
        state.ExecutedSteps.Add("Buscar solución");
        return state;
    }

    public IncidentWorkflowState RegisterIncident(IncidentWorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.HasKnownIncident || !string.IsNullOrWhiteSpace(state.Solution))
        {
            throw new InvalidOperationException("Sólo se registra un incidente sin explicación conocida.");
        }

        state.IncidentId = $"INC-{1001 + incidents.Count}";
        incidents.Add($"{state.IncidentId} | {state.Request.Service} | {state.Request.Description}");
        state.ExecutedSteps.Add("Registrar incidente");
        return state;
    }

    public IncidentWorkflowState Respond(IncidentWorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        ServiceStatus status = state.ServiceStatus
            ?? throw new InvalidOperationException("Debe consultarse el estado antes de responder.");

        state.Response = state.HasKnownIncident
            ? $"El servicio {state.Request.Service} está {status.Status}. " +
                $"Incidente conocido: {status.Incident}."
            : state.Solution is not null
                ? $"Solución conocida para {state.Request.ErrorCode}: {state.Solution}"
                : $"No hay una solución conocida para {state.Request.ErrorCode}. " +
                    $"Se registró el incidente {state.IncidentId}.";

        state.ExecutedSteps.Add("Responder");
        return state;
    }
}
