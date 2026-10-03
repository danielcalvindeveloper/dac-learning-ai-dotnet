using Microsoft.Extensions.AI;
using Lab02.ChatHistory;


LabConfiguration configuration;

try
{
    configuration = LabConfiguration.Load();
}
catch (InvalidOperationException ex)
{
    LabConsole.WriteError(ex.Message);
    return;
}

IChatClient chatClient =
    ChatClientFactory.Create(configuration);

const string firstMessage = "Mi nombre es Daniel";
const string secondMessage = "¿Cómo me llamo?";


Console.WriteLine("=== SIN MEMORIA ===");

Console.WriteLine($"Usuario: {firstMessage}");
ChatResponse r1 = await chatClient.GetResponseAsync(firstMessage);
Console.WriteLine($"Asistente: {r1.Text}");
Console.WriteLine();

Console.WriteLine($"Usuario: {secondMessage}");
ChatResponse r2 = await chatClient.GetResponseAsync(secondMessage);
Console.WriteLine($"Asistente: {r2.Text}");
Console.WriteLine();

Console.WriteLine("=== CON MEMORIA ===");

List<ChatMessage> history = [];

await ChatWithMemoryAsync(
    firstMessage,
    history,
    chatClient);

await ChatWithMemoryAsync(
    secondMessage,
    history,
    chatClient);

static async Task ChatWithMemoryAsync(
    string message,
    List<ChatMessage> history,
    IChatClient chatClient)
{
    Console.WriteLine($"Usuario: {message}");

    history.Add(
        new ChatMessage(ChatRole.User, message));

    ChatResponse response =
        await chatClient.GetResponseAsync(history);

    Console.WriteLine($"Asistente: {response.Text}");
    Console.WriteLine();

    foreach (ChatMessage responseMessage in response.Messages)
    {
        history.Add(responseMessage);
    }
}

