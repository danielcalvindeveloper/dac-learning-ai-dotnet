using Microsoft.Extensions.AI;

// API key utilizada exclusivamente por este ejemplo mínimo.
// Reemplazá el texto por una clave válida antes de ejecutar.
const string apiKey = "TU_API_KEY";

// Nombre del modelo al que queremos enviar la consulta.
const string model = "gpt-4o-mini";

// Crea el cliente concreto provisto por el SDK de OpenAI.
OpenAI.Chat.ChatClient openAiClient =
    new(model, apiKey);

// Adapta el cliente concreto a IChatClient,
// la abstracción común de Microsoft.Extensions.AI.
IChatClient chatClient =
    openAiClient.AsIChatClient();

// Define el mensaje que enviaremos al modelo.
const string prompt =
    "Explica en una sola oración qué es un modelo de lenguaje.";

// Envía el prompt al modelo y espera su respuesta.
ChatResponse response =
    await chatClient.GetResponseAsync(prompt);

// Muestra únicamente el texto generado por el modelo.
Console.WriteLine(response.Text);
