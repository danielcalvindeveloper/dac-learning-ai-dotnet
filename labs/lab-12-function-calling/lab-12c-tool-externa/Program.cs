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

using HttpClient httpClient = new()
{
    Timeout = TimeSpan.FromSeconds(20)
};
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("dac-learning-ai-dotnet/1.0");
httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");

CountryTools countryTools = new(httpClient);
AIFunction tool = AIFunctionFactory.Create(
    countryTools.ConsultarPais,
    name: nameof(CountryTools.ConsultarPais),
    description: "Consulta una API HTTP para obtener nombre, capital, región y población de un país. " +
        "Usá esta Tool para consultar los datos disponibles en el servicio, también si conocés el país. " +
        "El nombre debe ser el nombre completo del país en inglés, por ejemplo Argentina o Uruguay.");

ChatOptions options = new()
{
    Tools = [tool],
    ToolMode = ChatToolMode.RequireAny
};

string[] questions =
[
    "Consultá la API: ¿cuál es la capital de Argentina y cuántos habitantes tiene aproximadamente?",
    "Consultá la API: ¿cuál es la capital de Uruguay y cuántos habitantes tiene aproximadamente?",
    "Consultá el país llamado pais-inexistente-lab12. Usá ese nombre exacto; " +
        "si no existe, informá lo que devolvió la Tool."
];

Console.WriteLine("=== LAB 12c - TOOL EXTERNA ===");
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
            "Para estas consultas de países, obtené los datos mediante ConsultarPais. " +
            "No respondas desde tu conocimiento previo ni cambies el nombre solicitado. " +
            "Respondé en español con los datos devueltos. " +
            "La población es la cifra publicada por la API, no un conteo en tiempo real. " +
            "Si la Tool informa un error, explicalo sin inventar datos ni repetir la consulta."),
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
}
