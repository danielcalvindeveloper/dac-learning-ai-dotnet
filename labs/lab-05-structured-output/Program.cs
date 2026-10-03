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

Console.WriteLine("=== PERSONA ===");

const string textoPersona =
    "Mi nombre es Daniel y tengo 63 años.";

Console.WriteLine($"Texto: {textoPersona}");

Persona persona =
    await assistant.ExtraerPersonaAsync(textoPersona);

Console.WriteLine($"Nombre: {persona.Nombre}");
Console.WriteLine($"Edad: {persona.Edad}");
Console.WriteLine();

Console.WriteLine("=== PERSONAS ===");

const string textoPersonas = """
Mi nombre es Daniel y tengo 63 años.
Su nombre es Roberto, un joven aún, apenas 17 años.
30 años, qué bella que es Juana.
""";

Console.WriteLine("Texto:");
Console.WriteLine(textoPersonas);

Personas personas =
    await assistant.ExtraerPersonasAsync(textoPersonas);

foreach (Persona p in personas.PersonasEncontradas)
{
    Console.WriteLine(
        $"{p.Nombre} tiene {p.Edad} años");
}

Console.WriteLine();

Console.WriteLine("=== PRODUCTO ===");

const string textoProducto =
    "Notebook Lenovo ThinkPad.";

Console.WriteLine($"Texto: {textoProducto}");

Producto producto =
    await assistant.ExtraerProductoAsync(textoProducto);

Console.WriteLine($"Nombre: {producto.Nombre}");
Console.WriteLine($"Categoría: {producto.Categoria}");
