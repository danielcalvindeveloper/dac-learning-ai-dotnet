using Microsoft.Extensions.AI;
using Lab01.HelloLlm;

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

const string prompt =
    "Explica en una sola oración qué es un modelo de lenguaje.";

LabConsole.WriteRequest(
    configuration,
    prompt);

ChatResponse response =
    await chatClient.GetResponseAsync(prompt);

LabConsole.WriteResponse(response.Text);
