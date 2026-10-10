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

using IChatClient chatClient = ChatClientFactory.Create(configuration);

AIFunction tool = AIFunctionFactory.Create(
    CalcularTotalConIva,
    name: nameof(CalcularTotalConIva),
    description:
        "Calcula el total de una compra sumando el IVA al importe sin impuesto. " +
        "Recibe importe y porcentaje de IVA (por ejemplo, 21 para el 21%).");

ChatOptions options = new()
{
    Tools = [tool]
};

const string question =
    "¿Cuánto debo pagar por una compra de $1000 si corresponde aplicar un IVA del 21%?";

List<ChatMessage> messages =
[
    new(ChatRole.System,
        "Para calcular un total con IVA, solicitá la Tool disponible. " +
        "Cuando recibas su resultado, respondé brevemente en español con el total."),
    new(ChatRole.User, question)
];

Console.WriteLine("=== LAB 11a - PRIMER TOOL EXPLÍCITO ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");
Console.WriteLine();
Console.WriteLine("Usuario:");
Console.WriteLine(question);
Console.WriteLine();

ChatResponse response = await chatClient.GetResponseAsync(messages, options);

// Conservamos el mensaje nativo para no perder metadata del proveedor
// al continuar la conversación.
foreach (ChatMessage message in response.Messages)
{
    if (message.RawRepresentation is OpenAI.Chat.ChatCompletion completion)
    {
        message.RawRepresentation =
            new OpenAI.Chat.AssistantChatMessage(completion);
    }
}

messages.AddRange(response.Messages);

FunctionCallContent[] functionCalls = response.Messages
    .SelectMany(message => message.Contents)
    .OfType<FunctionCallContent>()
    .ToArray();

Console.WriteLine("=== RESPUESTA DEL MODELO ===");

if (functionCalls.Length == 0)
{
    Console.WriteLine("El modelo no solicitó ejecutar una Tool.");
    Console.WriteLine(response.Text);
    Console.WriteLine(
        "El ciclo no se completó. Revisá el soporte de Tool Calling del proveedor/modelo.");
    Environment.ExitCode = 1;
    return;
}

if (functionCalls.Length != 1)
{
    Console.WriteLine("Este laboratorio espera una única solicitud de Tool.");
    Environment.ExitCode = 1;
    return;
}

FunctionCallContent functionCall = functionCalls[0];

Console.WriteLine("El modelo solicitó ejecutar una Tool.");
Console.WriteLine($"Tool: {functionCall.Name}");
Console.WriteLine($"CallId: {functionCall.CallId}");
Console.WriteLine("Argumentos:");

if (functionCall.Arguments is not null)
{
    foreach (var argument in functionCall.Arguments)
    {
        Console.WriteLine($"{argument.Key}: {argument.Value}");
    }
}

if (functionCall.Name != tool.Name)
{
    Console.WriteLine("La Tool solicitada no corresponde a la función disponible.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine();
Console.WriteLine("=== EJECUCIÓN .NET ===");
Console.WriteLine($"La aplicación ejecuta {tool.Name}(...) mediante InvokeAsync.");

AIFunctionArguments arguments = new(functionCall.Arguments);
object? result = await tool.InvokeAsync(arguments);

Console.WriteLine("Resultado:");
Console.WriteLine(result);

FunctionResultContent functionResult = new(functionCall.CallId, result);
messages.Add(new ChatMessage(ChatRole.Tool, [functionResult]));

Console.WriteLine();
Console.WriteLine("=== RESULTADO DEVUELTO AL MODELO ===");
Console.WriteLine($"CallId: {functionResult.CallId}");
Console.WriteLine(result);

// La segunda llamada sólo pide la respuesta final, sin ofrecer nuevas Tools.
ChatResponse finalResponse;

try
{
    finalResponse = await chatClient.GetResponseAsync(messages);
}
catch (System.ClientModel.ClientResultException ex)
{
    Console.WriteLine($"El proveedor rechazó la continuación del ciclo (HTTP {ex.Status}).");
    Console.WriteLine(ex.GetRawResponse()?.Content.ToString());
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine();
Console.WriteLine("=== RESPUESTA FINAL ===");

if (string.IsNullOrWhiteSpace(finalResponse.Text))
{
    Console.WriteLine("El modelo no devolvió una respuesta final de texto.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine(finalResponse.Text);

static decimal CalcularTotalConIva(decimal importe, decimal porcentaje)
{
    return importe + importe * porcentaje / 100m;
}
