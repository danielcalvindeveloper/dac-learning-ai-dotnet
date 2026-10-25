const string dataDirectory = "data";

Console.WriteLine("=== LAB 14a - WORKFLOW EXPLÍCITO ===");

foreach (IncidentScenario scenario in IncidentWorkflowData.Scenarios)
{
    IncidentWorkflowData data = new(Path.Combine(AppContext.BaseDirectory, dataDirectory));
    IncidentWorkflowState state = new(scenario.Request);

    Console.WriteLine();
    Console.WriteLine(scenario.Name);
    Console.WriteLine($"Reporte: {scenario.Request.Description}");

    // El estado viaja de un paso al siguiente; el orden lo controla explícitamente la aplicación.
    data.Validate(state);
    data.QueryServiceStatus(state);

    // Estas condiciones son las bifurcaciones: una incidencia conocida evita pasos innecesarios.
    if (!state.HasKnownIncident)
    {
        data.SearchSolution(state);

        if (state.Solution is null)
        {
            data.RegisterIncident(state);
        }
    }

    data.Respond(state);

    Console.WriteLine($"Pasos: {string.Join(" → ", state.ExecutedSteps)}");
    Console.WriteLine($"Respuesta: {state.Response}");
}
