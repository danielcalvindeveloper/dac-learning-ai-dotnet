using DotNetEnv;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

// Busca el .env común del proyecto recorriendo los directorios padres.
Env.TraversePath().Load();

ServiceCollection services = new();

// Registramos el cliente de IA una sola vez.
services.AddSingleton<IChatClient>(
    _ => AiClientFactory.CreateFromEnvironment());

// Registramos nuestra abstracción de aplicación.
services.AddTransient<IAssistant, Assistant>();

// Construimos el proveedor de servicios.
using ServiceProvider serviceProvider =
    services.BuildServiceProvider();

//  Obtenemos la instancia de nuestro asistente desde el contenedor de servicios.
IAssistant assistant =
    serviceProvider.GetRequiredService<IAssistant>();

// CHAT
Console.WriteLine("=== CHAT ===");
Console.WriteLine("Usuario: ¿Qué es Microsoft.Extensions.AI?");

string response = await assistant.ChatAsync(
    "¿Qué es Microsoft.Extensions.AI?"
);

Console.WriteLine($"Asistente: {response}");
Console.WriteLine();

// TRADUCIR
Console.WriteLine("=== TRADUCIR ===");

const string textoTraducir =
    "Las abstracciones describen el contrato.";

Console.WriteLine($"Usuario: {textoTraducir}");

response = await assistant.TraducirAsync(textoTraducir);

Console.WriteLine($"Asistente: {response}");
Console.WriteLine();

// RESUMIR
Console.WriteLine("=== RESUMIR ===");

const string textoResumir =
    "Microsoft.Extensions.AI proporciona abstracciones comunes " +
    "para trabajar con modelos de IA sin acoplar toda la aplicación " +
    "a un proveedor concreto.";

Console.WriteLine($"Usuario: {textoResumir}");

response = await assistant.ResumirAsync(textoResumir);

Console.WriteLine($"Asistente: {response}");
Console.WriteLine();
