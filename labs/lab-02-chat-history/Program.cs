using Microsoft.Extensions.AI;
using Lab02.ChatHistory;

LabConfiguration configuration;
// Cargar la configuración del laboratorio
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

// Crear el cliente de chat
IChatClient chatClient =
    ChatClientFactory.Create(configuration);

// Definir los mensajes del usuario
const string firstMessage = "Mi nombre es Daniel";
const string secondMessage = "¿Cómo me llamo?";

// Mostrar la conversación sin memoria
Console.WriteLine("=== SIN MEMORIA ===");
Console.WriteLine($"Usuario: {firstMessage}");
ChatResponse r1 = await chatClient.GetResponseAsync(firstMessage);
// Mostrar la respuesta del asistente
Console.WriteLine($"Asistente: {r1.Text}");
Console.WriteLine();

// Mostrar la segunda interacción sin memoria
Console.WriteLine($"Usuario: {secondMessage}");
ChatResponse r2 = await chatClient.GetResponseAsync(secondMessage);

// Mostrar la respuesta del asistente
Console.WriteLine($"Asistente: {r2.Text}");
Console.WriteLine();


// Mostrar la conversación con memoria
Console.WriteLine("=== CON MEMORIA ===");

// Inicializar el historial de mensajes
List<ChatMessage> history = [];

// Agregar el primer mensaje del usuario al historial
Console.WriteLine($"Usuario: {firstMessage}");
history.Add(
        new ChatMessage(ChatRole.User, firstMessage));

// Obtener la respuesta del asistente y agregarla al historial
ChatResponse response =
    await chatClient.GetResponseAsync(history);
Console.WriteLine($"Asistente: {response.Text}");
Console.WriteLine();

foreach (ChatMessage responseMessage in response.Messages)
{
    history.Add(responseMessage);
}

// Agregar el segundo mensaje del usuario al historial
Console.WriteLine($"Usuario: {secondMessage}");
history.Add(
        new ChatMessage(ChatRole.User, secondMessage));

// Obtener la respuesta del asistente y agregarla al historial
response =
    await chatClient.GetResponseAsync(history);
Console.WriteLine($"Asistente: {response.Text}");
Console.WriteLine();

foreach (ChatMessage responseMessage in response.Messages)
{
    history.Add(responseMessage);
}

