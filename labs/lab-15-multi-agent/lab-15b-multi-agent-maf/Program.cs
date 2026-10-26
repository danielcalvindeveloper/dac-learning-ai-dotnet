using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
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

ChatClientAgent triageAgent = CreateAgent<TriageResult>(
    chatClient,
    "TriageAgent",
    "Sos el agente de triage. Extraé del reporte el servicio, un resumen breve " +
    "y el código de error explícito, si existe. Los únicos servicios válidos son " +
    "'pagos' y 'transferencias'. No infieras un código que el reporte no indique.");

ChatClientAgent diagnosisAgent = CreateAgent<DiagnosisResult>(
    chatClient,
    "DiagnosisAgent",
    "Sos especialista en diagnóstico. Interpretá únicamente el resultado de triage " +
    "y las observaciones locales recibidas. No inventes estado, incidentes ni soluciones. " +
    "Devolvé un hallazgo breve y el siguiente paso recomendado, sin exponer razonamiento privado.");

ChatClientAgent resolutionAgent = CreateAgent<ResolutionResult>(
    chatClient,
    "ResolutionAgent",
    "Sos especialista en comunicación de soporte. Redactá una respuesta breve y útil " +
    "basada sólo en los datos recibidos. Si hay incidente conocido, informalo; si hay " +
    "solución conocida, comunicala; si se registró un incidente, incluye su número. " +
    "No inventes acciones ni promesas de recuperación.");

Console.WriteLine("=== LAB 15b - COORDINACIÓN MULTI-AGENT CON MAF ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");

foreach (IncidentScenario scenario in SupportData.Scenarios)
{
    SupportData supportData = new(Path.Combine(AppContext.BaseDirectory, "data"));
    Workflow workflow = CreateWorkflow(
        triageAgent,
        diagnosisAgent,
        resolutionAgent,
        supportData);

    Console.WriteLine();
    Console.WriteLine(scenario.Name);
    Console.WriteLine($"Reporte: {scenario.Report}");

    await using Run run = await InProcessExecution.RunAsync(
        workflow,
        new SupportCase(scenario));

    WorkflowEvent[] events = run.NewEvents.ToArray();

    foreach (ExecutorCompletedEvent step in events.OfType<ExecutorCompletedEvent>())
    {
        Console.WriteLine($"Executor MAF: {step.ExecutorId}");
    }

    SupportCase result = events
        .OfType<WorkflowOutputEvent>()
        .Select(output => output.Data)
        .OfType<SupportCase>()
        .Single();

    Console.WriteLine($"TriageAgent: {JsonSerializer.Serialize(RequireTriage(result))}");
    Console.WriteLine($"DiagnosisAgent: {JsonSerializer.Serialize(RequireDiagnosis(result))}");

    if (result.IncidentId is not null)
    {
        Console.WriteLine($"Operación local: incidente {result.IncidentId} registrado.");
    }

    Console.WriteLine($"ResolutionAgent: {RequireResolution(result).Response}");
}

static ChatClientAgent CreateAgent<T>(
    IChatClient chatClient,
    string name,
    string instructions) =>
    chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = name,
        ChatOptions = new ChatOptions
        {
            Instructions = instructions,
            ResponseFormat = ChatResponseFormat.ForJsonSchema<T>()
        }
    });

static Workflow CreateWorkflow(
    ChatClientAgent triageAgent,
    ChatClientAgent diagnosisAgent,
    ChatClientAgent resolutionAgent,
    SupportData supportData)
{
    AgentStepExecutor triage = new(
        "TriageAgent",
        async (state, cancellationToken) =>
        {
            AgentResponse<TriageResult> result = await triageAgent.RunAsync<TriageResult>(
                state.Scenario.Report,
                cancellationToken: cancellationToken);

            return state with { Triage = result.Result };
        });

    AgentStepExecutor diagnosis = new(
        "DiagnosisAgent",
        async (state, cancellationToken) =>
        {
            TriageResult triageResult = RequireTriage(state);
            ServiceStatus serviceStatus = supportData.GetServiceStatus(triageResult.Service);
            string? knownSolution = supportData.FindSolution(triageResult.ErrorCode);

            AgentResponse<DiagnosisResult> result = await diagnosisAgent.RunAsync<DiagnosisResult>(
                JsonSerializer.Serialize(
                    new DiagnosisInput(triageResult, serviceStatus, knownSolution)),
                cancellationToken: cancellationToken);

            string? incidentId = null;

            if (string.IsNullOrWhiteSpace(serviceStatus.Incident)
                && string.IsNullOrWhiteSpace(knownSolution))
            {
                incidentId = supportData.RegisterIncident(
                    triageResult.Service,
                    state.Scenario.Report,
                    serviceStatus,
                    knownSolution);
            }

            return state with
            {
                ServiceStatus = serviceStatus,
                KnownSolution = knownSolution,
                Diagnosis = result.Result,
                IncidentId = incidentId
            };
        });

    AgentStepExecutor resolution = new(
        "ResolutionAgent",
        async (state, cancellationToken) =>
        {
            TriageResult triageResult = RequireTriage(state);
            ServiceStatus serviceStatus = state.ServiceStatus
                ?? throw new InvalidOperationException("DiagnosisAgent no devolvió el estado.");
            DiagnosisResult diagnosisResult = RequireDiagnosis(state);

            AgentResponse<ResolutionResult> result = await resolutionAgent.RunAsync<ResolutionResult>(
                JsonSerializer.Serialize(
                    new ResolutionInput(
                        state.Scenario.Report,
                        triageResult,
                        serviceStatus,
                        state.KnownSolution,
                        diagnosisResult,
                        state.IncidentId)),
                cancellationToken: cancellationToken);

            return state with { Resolution = result.Result };
        });

    return new WorkflowBuilder(triage)
        .AddEdge(triage, diagnosis)
        .AddEdge(diagnosis, resolution)
        .WithOutputFrom(resolution)
        .Build();
}

static TriageResult RequireTriage(SupportCase state) =>
    state.Triage
        ?? throw new InvalidOperationException("TriageAgent no devolvió un resultado.");

static DiagnosisResult RequireDiagnosis(SupportCase state) =>
    state.Diagnosis
        ?? throw new InvalidOperationException("DiagnosisAgent no devolvió un resultado.");

static ResolutionResult RequireResolution(SupportCase state) =>
    state.Resolution
        ?? throw new InvalidOperationException("ResolutionAgent no devolvió un resultado.");

internal sealed class AgentStepExecutor(
    string id,
    Func<SupportCase, CancellationToken, ValueTask<SupportCase>> handleAsync)
    : Executor<SupportCase, SupportCase>(id)
{
    public override ValueTask<SupportCase> HandleAsync(
        SupportCase message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default) =>
        handleAsync(message, cancellationToken);
}
