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
    .UseFunctionInvocation(configure: client => client.IncludeDetailedErrors = true)
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
    "Explicá en una oración qué significa aplicar un descuento.",
    "Usá CalcularDescuento para un importe de $1000 y un descuento del 150%. " +
    "Conservá esos valores; si la Tool los rechaza, explicá el error sin corregirlos."
];

Console.WriteLine("=== LAB 12b - DECISIÓN Y ERRORES ===");
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
            "Para cálculos numéricos solicitá la Tool correspondiente con los valores indicados. " +
            "Para explicaciones conceptuales respondé directamente. " +
            "Si una Tool devuelve un error, explicalo sin cambiar los argumentos ni inventar un resultado."),
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
