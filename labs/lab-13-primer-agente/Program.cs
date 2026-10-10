using Microsoft.Extensions.AI;

LabConfiguration configuration;

try
{
    configuration = LabConfiguration.Load();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    Environment.ExitCode = 1;
    return;
}

using IChatClient chatClient = new ChatClientBuilder(
    ChatClientFactory.Create(configuration))
    .UseFunctionInvocation(configure: client =>
    {
        client.TerminateOnUnknownCalls = true;
        client.MaximumConsecutiveErrorsPerRequest = 0;
        client.FunctionInvoker = async (context, cancellationToken) =>
        {
            // Devolver el control al while después de la acción, sin pedir otra decisión.
            context.Terminate = true;

            if (context.FunctionCount != 1)
            {
                throw new InvalidOperationException("El laboratorio admite una sola Tool por decisión.");
            }

            Console.WriteLine("Decisión del modelo / Tool:");
            Console.WriteLine(context.Function.Name);
            Console.WriteLine("Argumentos:");

            foreach (var argument in context.Arguments)
            {
                Console.WriteLine($"{argument.Key}: {argument.Value}");
            }

            return await context.Function.InvokeAsync(context.Arguments, cancellationToken);
        };
    })
    .Build();

const int maxIterations = 5;

string[] reports =
[
    "No puedo usar transferencias.",
    "Pagos devuelve error 503.",
    "Pagos devuelve error X91."
];

Console.WriteLine("=== LAB 13 - PRIMER AGENTE ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");

foreach (string report in reports)
{
    AgentState state = new()
    {
        Objective = "Diagnosticar el incidente y proporcionar una acción útil al usuario."
    };

    Console.WriteLine();
    Console.WriteLine($"OBJETIVO: {state.Objective}");
    Console.WriteLine($"REPORTE: {report}");

    try
    {
        SupportTools tools = new(Path.Combine(AppContext.BaseDirectory, "data"));

        ChatOptions options = new()
        {
            AllowMultipleToolCalls = false,
            Tools =
            [
                AIFunctionFactory.Create(
                    tools.ConsultarEstadoServicio,
                    name: nameof(SupportTools.ConsultarEstadoServicio),
                    description: "Consulta el estado actual local de un servicio por su nombre, " +
                        "por ejemplo pagos o transferencias, y su incidente conocido si existe."),
                AIFunctionFactory.Create(
                    tools.BuscarSolucion,
                    name: nameof(SupportTools.BuscarSolucion),
                    description: "Busca una recomendación local por concepto exacto: " +
                        "código de error, por ejemplo 503 o 401, o timeout."),
                AIFunctionFactory.Create(
                    tools.RegistrarIncidente,
                    name: nameof(SupportTools.RegistrarIncidente),
                    description: "Registra en memoria un incidente sin explicación suficiente " +
                        "y devuelve su número. Recibe el servicio y la descripción del problema.")
            ]
        };

        List<ChatMessage> messages =
        [
            new(ChatRole.System,
                "Sos un agente de soporte técnico. " +
                $"Tu objetivo es: {state.Objective} " +
                "Observá el estado del servicio antes de diagnosticar. " +
                "Usá las Tools cuando necesites consultar estado, buscar una solución o registrar un incidente. " +
                "Solicitá una sola Tool por decisión y observá su resultado antes de decidir otra acción. " +
                "No registres un incidente si ya existe una explicación suficiente. " +
                "Un incidente conocido ya explica la indisponibilidad: informalo y finalizá, " +
                "sin duplicar su registro ni buscar otra solución. " +
                "Si falta una explicación, registralo para seguimiento. " +
                "Finalizá con una acción útil basada en las observaciones, " +
                "sin inventar resultados ni soluciones ausentes en ellas."),
            new(ChatRole.User, report)
        ];

        while (!state.Completed && state.Iteration < maxIterations)
        {
            state.Iteration++;
            Console.WriteLine();
            Console.WriteLine($"--- ITERACIÓN {state.Iteration} / {maxIterations} ---");

            ChatResponse response = await chatClient.GetResponseAsync(messages, options);
            messages.AddRange(response.Messages);

            FunctionResultContent[] results = response.Messages
                .SelectMany(message => message.Contents)
                .OfType<FunctionResultContent>()
                .ToArray();

            foreach (FunctionResultContent result in results)
            {
                string observation = result.Result?.ToString() ?? "La Tool no devolvió contenido.";
                state.Observations.Add(observation);

                Console.WriteLine("Resultado / observación agregada:");
                Console.WriteLine(observation);
            }

            if (results.Length > 0)
            {
                continue;
            }

            if (response.Messages.SelectMany(message => message.Contents)
                .OfType<FunctionCallContent>().Any())
            {
                throw new InvalidOperationException("El modelo solicitó una Tool no ejecutable.");
            }

            if (string.IsNullOrWhiteSpace(response.Text))
            {
                throw new InvalidOperationException("El modelo no devolvió una acción ni una respuesta final.");
            }

            state.Completed = true;
            Console.WriteLine("Decisión del modelo: finalizar.");
            Console.WriteLine("=== OBJETIVO COMPLETADO SEGÚN EL MODELO ===");
            Console.WriteLine(response.Text);
        }

        if (!state.Completed)
        {
            Console.WriteLine("=== LÍMITE ALCANZADO: OBJETIVO NO COMPLETADO ===");
            Environment.ExitCode = 1;
        }
    }
    catch (System.ClientModel.ClientResultException ex)
    {
        Console.WriteLine($"El proveedor LLM rechazó la solicitud (HTTP {ex.Status}).");
        Environment.ExitCode = 1;
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException
        or IOException or System.Text.Json.JsonException)
    {
        Console.WriteLine($"No se pudo completar el diagnóstico: {ex.Message}");
        Environment.ExitCode = 1;
    }

    Console.WriteLine($"Iteraciones: {state.Iteration}");
    Console.WriteLine($"Observaciones: {state.Observations.Count}");
    Console.WriteLine($"Completado: {state.Completed}");
}
