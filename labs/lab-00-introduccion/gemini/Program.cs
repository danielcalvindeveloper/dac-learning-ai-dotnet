using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

// API key utilizada exclusivamente por este ejemplo mínimo.
// Reemplazá el texto por una clave válida antes de ejecutar.
const string apiKey = "TU_API_KEY";

// Nombre del modelo de Gemini que queremos utilizar.
const string model = "gemini-3.5-flash-lite";

// Define el endpoint compatible con OpenAI expuesto por Gemini.
OpenAIClientOptions options = new()
{
    Endpoint = new Uri(
        "https://generativelanguage.googleapis.com/v1beta/openai/")
};

// Crea el cliente concreto indicando modelo, credencial y endpoint.
OpenAI.Chat.ChatClient geminiClient = new(
    model,
    new ApiKeyCredential(apiKey),
    options);

// Adapta el cliente concreto a IChatClient,
// la misma abstracción utilizada en el ejemplo de OpenAI.
IChatClient chatClient =
    geminiClient.AsIChatClient();

// Define el mensaje que enviaremos al modelo.
const string prompt =
    "Explica en una sola oración qué es un modelo de lenguaje.";

// Envía el prompt al modelo y espera su respuesta.
ChatResponse response =
    await chatClient.GetResponseAsync(prompt);

// Muestra únicamente el texto generado por el modelo.
Console.WriteLine(response.Text);
