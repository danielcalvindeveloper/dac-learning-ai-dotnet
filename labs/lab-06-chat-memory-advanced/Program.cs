using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddSingleton<IChatClient>(
    _ => AiClientFactory.CreateFromEnvironment());

services.AddSingleton<
    IChatMemoryStore,
    InMemoryChatMemoryStore>();

services.AddTransient<IAssistant, Assistant>();

using ServiceProvider serviceProvider =
    services.BuildServiceProvider();

IAssistant assistant;

try
{
    assistant =
        serviceProvider.GetRequiredService<IAssistant>();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    return;
}

Console.WriteLine("=== SESIÓN A ===");

await MostrarDialogoAsync(
    assistant,
    "sesion-a",
    "Mi nombre es Daniel");

await MostrarDialogoAsync(
    assistant,
    "sesion-a",
    "¿Cómo me llamo?");

Console.WriteLine();

Console.WriteLine("=== SESIÓN B ===");

await MostrarDialogoAsync(
    assistant,
    "sesion-b",
    "Mi nombre es Roberto");

await MostrarDialogoAsync(
    assistant,
    "sesion-b",
    "¿Cómo me llamo?");

Console.WriteLine();

Console.WriteLine("=== SESIÓN A (AISLADA) ===");

await MostrarDialogoAsync(
    assistant,
    "sesion-a",
    "¿Cómo me llamo?");

static async Task MostrarDialogoAsync(
    IAssistant assistant,
    string sessionId,
    string message)
{
    Console.WriteLine(
        $"[{sessionId}] Usuario: {message}");

    string response =
        await assistant.ChatAsync(
            sessionId,
            message);

    Console.WriteLine(
        $"[{sessionId}] Asistente: {response}");
}
