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

ChatOptions options = new()
{
    Tools =
    [
        AIFunctionFactory.Create(
            LocalTools.CalcularTotalConIva,
            name: nameof(LocalTools.CalcularTotalConIva),
            description: "Calcula el total de una compra sumando el IVA al importe sin impuesto. " +
                "El porcentaje se expresa como 21 para el 21%."),
        AIFunctionFactory.Create(
            LocalTools.CalcularDescuento,
            name: nameof(LocalTools.CalcularDescuento),
            description: "Calcula cuánto se paga después de aplicar un descuento al importe. " +
                "El porcentaje se expresa como 15 para el 15%."),
        AIFunctionFactory.Create(
            LocalTools.ConvertirCelsiusAFahrenheit,
            name: nameof(LocalTools.ConvertirCelsiusAFahrenheit),
            description: "Convierte una temperatura en grados Celsius a grados Fahrenheit.")
    ]
};

string[] questions =
[
    "¿Cuánto debo pagar por una compra de $1000 con 21% de IVA?",
    "¿Cuánto pago por un producto de $2500 con un descuento del 15%?",
    "¿Cuántos grados Fahrenheit son 30 grados Celsius?"
];

Console.WriteLine("=== LAB 12a - MÚLTIPLES TOOLS ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");

foreach (string question in questions)
{
    Console.WriteLine();
    Console.WriteLine("Usuario:");
    Console.WriteLine(question);

    List<ChatMessage> messages =
    [
        new(ChatRole.System,
            "Para cálculos numéricos solicitá la Tool correspondiente. " +
            "Usá su resultado para responder brevemente en español."),
        new(ChatRole.User, question)
    ];

    try
    {
        ChatResponse response = await chatClient.GetResponseAsync(messages, options);
        Console.WriteLine("=== RESPUESTA FINAL ===");

        if (string.IsNullOrWhiteSpace(response.Text))
        {
            Console.WriteLine("El modelo no devolvió una respuesta final de texto.");
            Environment.ExitCode = 1;
            continue;
        }

        Console.WriteLine(response.Text);
    }
    catch (System.ClientModel.ClientResultException ex)
    {
        Console.WriteLine($"El proveedor LLM rechazó la solicitud (HTTP {ex.Status}).");
        Environment.ExitCode = 1;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"La invocación terminó con un error de argumentos: {ex.Message}");
        Environment.ExitCode = 1;
    }
}
