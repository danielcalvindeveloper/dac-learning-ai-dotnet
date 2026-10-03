using Microsoft.Extensions.AI;
using Lab01.HelloLlm;

LabConfiguration configuration;

try
{
    configuration = LabConfiguration.Load();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
    return;
}

Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");
Console.WriteLine();

IChatClient chatClient =
    ChatClientFactory.Create(configuration);

const string prompt =
    "Explica en una sola oración qué es un modelo de lenguaje.";

Console.WriteLine($"Prompt: {prompt}");
ChatResponse response =
    await chatClient.GetResponseAsync(prompt);
Console.WriteLine($"Respuesta: {response.Text}");

