using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddSingleton<IChatClient>(
    _ => AiClientFactory.CreateFromEnvironment());

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

/*
 * ==========================================================
 * EJEMPLO 1 - TRADUCCIÓN
 * ==========================================================
 */
Console.WriteLine("=== TRADUCCIÓN ===");

const string idioma = "inglés";
const string textoTraducir =
    "Las abstracciones describen el prompt.";

Console.WriteLine($"Idioma: {idioma}");
Console.WriteLine($"Texto: {textoTraducir}");

string response = await assistant.TraducirAsync(
    idioma,
    textoTraducir);

Console.WriteLine($"Asistente: {response}");
Console.WriteLine();

/*
 * ==========================================================
 * EJEMPLO 2 - RESUMEN
 * ==========================================================
 */
Console.WriteLine("=== RESUMEN ===");

const int lineas = 2;

const string textoResumir = """
Microsoft.Extensions.AI proporciona abstracciones comunes para trabajar con modelos de lenguaje.
Permite desacoplar la aplicación del proveedor concreto.
IChatClient representa una interfaz común para servicios de chat.
Dependency Injection permite registrar y resolver estas abstracciones.
Los Prompt Templates permiten construir mensajes dinámicos utilizando variables.
""";

Console.WriteLine($"Máximo de líneas: {lineas}");
Console.WriteLine("Texto:");
Console.WriteLine(textoResumir);

response = await assistant.ResumirAsync(
    lineas,
    textoResumir);

Console.WriteLine($"Asistente: {response}");
Console.WriteLine();

/*
 * ==========================================================
 * EJEMPLO 3 - CAMBIO DE ROL
 * ==========================================================
 */
Console.WriteLine("=== CONSULTA COMO PROFESOR ===");

const string rolProfesor =
    "profesor de matemáticas";

const string preguntaProfesor =
    "¿Qué es una derivada?";

Console.WriteLine($"Rol: {rolProfesor}");
Console.WriteLine($"Pregunta: {preguntaProfesor}");

response = await assistant.ConsultarAsync(
    rolProfesor,
    preguntaProfesor);

Console.WriteLine($"Asistente: {response}");
Console.WriteLine();

Console.WriteLine("=== CONSULTA COMO ABOGADO ===");

const string rolAbogado =
    "abogado";

const string preguntaAbogado =
    "¿Qué es un contrato?";

Console.WriteLine($"Rol: {rolAbogado}");
Console.WriteLine($"Pregunta: {preguntaAbogado}");

response = await assistant.ConsultarAsync(
    rolAbogado,
    preguntaAbogado);

Console.WriteLine($"Asistente: {response}");
