using System.Text.Json;
using Microsoft.Extensions.AI;

LabConfiguration configuration;

try
{
    configuration = LabConfiguration.Load();
}
catch (InvalidOperationException exception)
{
    Console.WriteLine(exception.Message);
    Environment.ExitCode = 1;
    return;
}

using IChatClient chatClient = ChatClientFactory.Create(configuration);

Console.WriteLine("=== LAB 15a - COORDINACIÓN MULTI-AGENT EXPLÍCITA ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");

foreach (IncidentScenario scenario in SupportData.Scenarios)
{
    SupportData supportData = new(Path.Combine(AppContext.BaseDirectory, "data"));

    Console.WriteLine();
    Console.WriteLine(scenario.Name);
    Console.WriteLine($"Reporte: {scenario.Report}");

    TriageResult triage = await RunTriageAgentAsync(chatClient, scenario.Report);
    Console.WriteLine($"TriageAgent: {JsonSerializer.Serialize(triage)}");

    ServiceStatus serviceStatus = supportData.GetServiceStatus(triage.Service);
    string? knownSolution = supportData.FindSolution(triage.ErrorCode);

    DiagnosisResult diagnosis = await RunDiagnosisAgentAsync(
        chatClient,
        new DiagnosisInput(triage, serviceStatus, knownSolution));
    Console.WriteLine($"DiagnosisAgent: {JsonSerializer.Serialize(diagnosis)}");

    string? incidentId = null;

    if (string.IsNullOrWhiteSpace(serviceStatus.Incident)
        && string.IsNullOrWhiteSpace(knownSolution))
    {
        incidentId = supportData.RegisterIncident(
            triage.Service,
            scenario.Report,
            serviceStatus,
            knownSolution);
        Console.WriteLine($"Operación local: incidente {incidentId} registrado.");
    }

    ResolutionResult resolution = await RunResolutionAgentAsync(
        chatClient,
        new ResolutionInput(
            scenario.Report,
            triage,
            serviceStatus,
            knownSolution,
            diagnosis,
            incidentId));

    Console.WriteLine($"ResolutionAgent: {resolution.Response}");
}

static async Task<TriageResult> RunTriageAgentAsync(IChatClient chatClient, string report)
{
    TriageResult result = await AskAgentAsync<TriageResult>(
        chatClient,
        "Sos el agente de triage. Extraé del reporte el servicio, un resumen breve " +
        "y el código de error explícito, si existe. Los únicos servicios válidos son " +
        "'pagos' y 'transferencias'. No infieras un código que el reporte no indique. " +
        "Usá errorCode como cadena vacía si el reporte no incluye un código. " +
        "Devolvé sólo el objeto solicitado.",
        report);

    if (string.IsNullOrWhiteSpace(result.Service)
        || string.IsNullOrWhiteSpace(result.Summary)
        || result.ErrorCode is null)
    {
        throw new InvalidOperationException(
            "TriageAgent devolvió campos vacíos; verificá que el modelo admita structured outputs.");
    }

    return result;
}

static Task<DiagnosisResult> RunDiagnosisAgentAsync(
    IChatClient chatClient,
    DiagnosisInput input) =>
    AskAgentAsync<DiagnosisResult>(
        chatClient,
        "Sos especialista en diagnóstico. Interpretá únicamente el resultado de triage " +
        "y las observaciones locales recibidas. No inventes estado, incidentes ni soluciones. " +
        "Devolvé un hallazgo breve y el siguiente paso recomendado, sin exponer razonamiento privado.",
        input);

static Task<ResolutionResult> RunResolutionAgentAsync(
    IChatClient chatClient,
    ResolutionInput input) =>
    AskAgentAsync<ResolutionResult>(
        chatClient,
        "Sos especialista en comunicación de soporte. Redactá una respuesta breve y útil " +
        "basada sólo en los datos recibidos. Si hay incidente conocido, informalo; si hay " +
        "solución conocida, comunicala; si se registró un incidente, incluye su número. " +
        "No inventes acciones ni promesas de recuperación.",
        input);

static async Task<T> AskAgentAsync<T>(
    IChatClient chatClient,
    string instructions,
    object input)
{
    ChatResponse response = await chatClient.GetResponseAsync(
        [
            new ChatMessage(ChatRole.System, instructions),
            new ChatMessage(
                ChatRole.User,
                input is string text
                    ? text
                    : JsonSerializer.Serialize(input, JsonSerializerOptions.Web))
        ],
        new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema<T>()
        });

    return JsonSerializer.Deserialize<T>(response.Text)
        ?? throw new InvalidOperationException(
            $"El agente {typeof(T).Name} devolvió una respuesta vacía.");
}
