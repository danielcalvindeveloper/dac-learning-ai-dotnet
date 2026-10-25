using Microsoft.Agents.AI.Workflows;

const string dataDirectory = "data";

Console.WriteLine("=== LAB 14b - WORKFLOW CON MICROSOFT AGENT FRAMEWORK ===");

foreach (IncidentScenario scenario in IncidentWorkflowData.Scenarios)
{
    IncidentWorkflowData data = new(Path.Combine(AppContext.BaseDirectory, dataDirectory));
    Workflow workflow = CreateWorkflow(data);

    Console.WriteLine();
    Console.WriteLine(scenario.Name);
    Console.WriteLine($"Reporte: {scenario.Request.Description}");

    await using Run run = await InProcessExecution.RunAsync(
        workflow,
        new IncidentWorkflowState(scenario.Request));

    IncidentWorkflowState state = run.NewEvents
        .OfType<WorkflowOutputEvent>()
        .Select(output => output.Data)
        .OfType<IncidentWorkflowState>()
        .Single();

    Console.WriteLine($"Pasos: {string.Join(" → ", state.ExecutedSteps)}");
    Console.WriteLine($"Respuesta: {state.Response}");
}

static Workflow CreateWorkflow(IncidentWorkflowData data)
{
    // Los ejecutores reutilizan los pasos de dominio; MAF aporta el grafo y su enrutamiento.
    var validate = ((Func<IncidentWorkflowState, IncidentWorkflowState>)data.Validate)
        .BindAsExecutor("ValidarReporte");
    var queryStatus = ((Func<IncidentWorkflowState, IncidentWorkflowState>)data.QueryServiceStatus)
        .BindAsExecutor("ConsultarEstado");
    var searchSolution = ((Func<IncidentWorkflowState, IncidentWorkflowState>)data.SearchSolution)
        .BindAsExecutor("BuscarSolucion");
    var registerIncident = ((Func<IncidentWorkflowState, IncidentWorkflowState>)data.RegisterIncident)
        .BindAsExecutor("RegistrarIncidente");
    var respond = ((Func<IncidentWorkflowState, IncidentWorkflowState>)data.Respond)
        .BindAsExecutor("Responder");

    // El branching se declara mediante edges, en lugar de if/else en el programa de orquestación.
    return new WorkflowBuilder(validate)
        .AddEdge(validate, queryStatus)
        .AddEdge<IncidentWorkflowState>(
            queryStatus,
            searchSolution,
            condition: state => state is not null && !state.HasKnownIncident)
        .AddEdge<IncidentWorkflowState>(
            queryStatus,
            respond,
            condition: state => state is not null && state.HasKnownIncident)
        .AddEdge<IncidentWorkflowState>(
            searchSolution,
            registerIncident,
            condition: state => state is not null && state.Solution is null)
        .AddEdge<IncidentWorkflowState>(
            searchSolution,
            respond,
            condition: state => state is not null && state.Solution is not null)
        .AddEdge(registerIncident, respond)
        .WithOutputFrom(respond)
        .Build();
}
