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
    .UseFunctionInvocation()
    .Build();

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

Console.WriteLine("=== LAB 11b - INVOCACIÓN AUTOMÁTICA DE TOOLS ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");
Console.WriteLine();
Console.WriteLine("Usuario:");
Console.WriteLine(question);
Console.WriteLine();

ChatResponse response;

try
{
    response = await chatClient.GetResponseAsync(messages, options);
}
catch (System.ClientModel.ClientResultException ex)
{
    Console.WriteLine($"El proveedor rechazó la solicitud (HTTP {ex.Status}).");
    Console.WriteLine(ex.GetRawResponse()?.Content.ToString());
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine();
Console.WriteLine("=== RESPUESTA FINAL ===");

if (string.IsNullOrWhiteSpace(response.Text))
{
    Console.WriteLine("El modelo no devolvió una respuesta final de texto.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine(response.Text);

static decimal CalcularTotalConIva(decimal importe, decimal porcentaje)
{
    decimal total = importe + importe * porcentaje / 100m;

    Console.WriteLine("=== TOOL EJECUTADA POR .NET ===");
    Console.WriteLine($"importe: {importe}");
    Console.WriteLine($"porcentaje: {porcentaje}");
    Console.WriteLine($"resultado: {total}");

    return total;
}
