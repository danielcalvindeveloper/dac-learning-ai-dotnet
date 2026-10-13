using Microsoft.Extensions.AI;

// Leemos la configuración del proveedor y del modelo que usaremos en las llamadas.
var configuration = LabConfiguration.Load();
// Creamos el cliente de chat; using libera sus recursos al terminar el programa.
using IChatClient chatClient = ChatClientFactory.Create(configuration);


// Convertimos el método C# en una Tool: el modelo verá su nombre, descripción y parámetros.
// Esto todavía no ejecuta el cálculo; sólo describe una capacidad disponible.
AIFunction tool = AIFunctionFactory.Create(
    CalcularTotalConIva,
    name: nameof(CalcularTotalConIva),
    description:
        "Calcula el total de una compra sumando el IVA al importe sin impuesto. " +
        "Recibe importe y porcentaje de IVA (por ejemplo, 21 para el 21%).");

// Ofrecemos esta Tool en la primera llamada para que el modelo pueda solicitar su ejecución.
ChatOptions options = new()
{
    Tools = [tool]
};

const string question = "¿Cuánto debo pagar por una compra de $1000 si corresponde aplicar un IVA del 21%?";

// El historial empieza con una instrucción de comportamiento (System) y la pregunta (User).
// Después agregaremos la solicitud del modelo y el resultado de la Tool a esta misma lista.
List<ChatMessage> messages =
[
    new(ChatRole.System,"Para calcular un total con IVA, solicitá la Tool disponible. " +
                        "Cuando recibas su resultado, respondé brevemente en español con el total."),
    new(ChatRole.User, question)
];

Console.WriteLine("=== LAB 11a - PRIMER TOOL EXPLÍCITO ===");
Console.WriteLine($"Proveedor: {configuration.ProviderName}");
Console.WriteLine($"Modelo: {configuration.Model}");
Console.WriteLine();
Console.WriteLine("Usuario:");
Console.WriteLine(question);
Console.WriteLine();

// Primera llamada: enviamos el historial y la definición de la Tool al modelo.
// El modelo puede devolver una solicitud con argumentos; nuestra aplicación tendrá que ejecutarla.
ChatResponse response = await chatClient.GetResponseAsync(messages, options);

// Preparamos la respuesta para reenviarla sin perder datos que el proveedor necesita.
// El detalle de compatibilidad está en el método al final de este archivo.
PrepararMensajesParaReenvio(response);

// Guardamos los mensajes completos del modelo, incluida su solicitud de Tool, en el historial.
messages.AddRange(response.Messages);

// Reunimos los contenidos de todos los mensajes y seleccionamos sólo las llamadas a funciones.
// FunctionCallContent contiene el nombre de la Tool, sus argumentos y el identificador de la llamada.
FunctionCallContent[] functionCalls = response.Messages
    .SelectMany(message => message.Contents)
    .OfType<FunctionCallContent>()
    .ToArray();

Console.WriteLine("=== RESPUESTA DEL MODELO ===");

// Si el modelo no pidió ninguna Tool, no podemos mostrar el ciclo que enseña este laboratorio.
if (functionCalls.Length == 0)
{
    Console.WriteLine("El modelo no solicitó ejecutar una Tool.");
    Console.WriteLine(response.Text);
    Console.WriteLine( "El ciclo no se completó. Revisá el soporte de Tool Calling del proveedor/modelo.");
    Environment.ExitCode = 1;
    return;
}

// Este ejemplo procesa una sola llamada; detenemos el flujo si el modelo solicita varias.
if (functionCalls.Length != 1)
{
    Console.WriteLine("Este laboratorio espera una única solicitud de Tool.");
    Environment.ExitCode = 1;
    return;
}

// Tomamos la única solicitud recibida: todavía es una petición, no un cálculo ejecutado.
FunctionCallContent functionCall = functionCalls[0];

Console.WriteLine("El modelo solicitó ejecutar una Tool.");
Console.WriteLine($"Tool: {functionCall.Name}");
Console.WriteLine($"CallId: {functionCall.CallId}");
Console.WriteLine("Argumentos:");

// Mostramos los valores que eligió el modelo para los parámetros importe y porcentaje.
if (functionCall.Arguments is not null)
{
    foreach (var argument in functionCall.Arguments)
    {
        Console.WriteLine($"{argument.Key}: {argument.Value}");
    }
}

// Verificamos que la solicitud corresponda a la función que realmente ofrecemos.
if (functionCall.Name != tool.Name)
{
    Console.WriteLine("La Tool solicitada no corresponde a la función disponible.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine();
Console.WriteLine("=== EJECUCIÓN .NET ===");
Console.WriteLine($"La aplicación ejecuta {tool.Name}(...) mediante InvokeAsync.");

// Adaptamos los argumentos recibidos al formato que acepta AIFunction.
AIFunctionArguments arguments = new(functionCall.Arguments);
// Aquí sí ejecutamos el método C# mediante la Tool; el resultado lo calcula .NET.
object? result = await tool.InvokeAsync(arguments);

Console.WriteLine("Resultado:");
Console.WriteLine(result);

// El mismo CallId vincula este resultado con la solicitud concreta que hizo el modelo.
FunctionResultContent functionResult = new(functionCall.CallId, result);
// Agregamos el resultado con rol Tool para que el modelo lo reciba en la siguiente llamada.
messages.Add(new ChatMessage(ChatRole.Tool, [functionResult]));

Console.WriteLine();
Console.WriteLine("=== RESULTADO DEVUELTO AL MODELO ===");
Console.WriteLine($"CallId: {functionResult.CallId}");
Console.WriteLine(result);

// La segunda llamada sólo pide la respuesta final, sin ofrecer nuevas Tools.
ChatResponse finalResponse;

try
{
    // Reenviamos el historial: pregunta, solicitud del asistente y resultado de la Tool.
    // Sin options no ofrecemos nuevas Tools; esperamos que el modelo redacte la respuesta.
    finalResponse = await chatClient.GetResponseAsync(messages);
}
catch (System.ClientModel.ClientResultException ex)
{
    // Si el proveedor rechaza la continuación, mostramos el diagnóstico y terminamos con error.
    Console.WriteLine($"El proveedor rechazó la continuación del ciclo (HTTP {ex.Status}).");
    Console.WriteLine(ex.GetRawResponse()?.Content.ToString());
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine();
Console.WriteLine("=== RESPUESTA FINAL ===");

// Comprobamos que la segunda llamada haya producido texto para mostrar al usuario.
if (string.IsNullOrWhiteSpace(finalResponse.Text))
{
    Console.WriteLine("El modelo no devolvió una respuesta final de texto.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine(finalResponse.Text);


// Esta es la lógica local de la Tool: suma al importe el IVA expresado como porcentaje.
// El sufijo m indica un literal decimal, el mismo tipo que usan los parámetros monetarios.
static decimal CalcularTotalConIva(decimal importe, decimal porcentaje)
{
    return importe + importe * porcentaje / 100m;
}


// Para devolver el resultado del cálculo, también reenviamos el mensaje donde el modelo pidió la Tool.
// Gemini puede exigir una firma interna (thought_signature) que vino junto con esa solicitud.
// Usamos la respuesta original para conservarla; no necesitamos interpretar ni crear esa firma.
static void PrepararMensajesParaReenvio(ChatResponse response)
{
    // El SDK (la librería que comunica nuestra aplicación con el proveedor) distingue dos formatos:
    // ChatCompletion es la respuesta recibida del LLM; AssistantChatMessage es un mensaje del modelo que podemos enviar.
    // Recorremos los mensajes para convertir ChatCompletion a AssistantChatMessage y conservar sus datos originales.
    foreach (ChatMessage message in response.Messages)
    {
        // ChatMessage es el formato común de Microsoft.Extensions.AI; RawRepresentation guarda el objeto propio del SDK.
        // Este if comprueba si el objeto guardado en RawRepresentation es de tipo ChatCompletion.
        // Si coincide, llamamos completion a esa respuesta y usamos sus datos para crear un AssistantChatMessage.
        // Así pasamos del formato de respuesta recibida al de mensaje del asistente que podemos reenviar.
        if (message.RawRepresentation is OpenAI.Chat.ChatCompletion completion)
        {
            // Creamos un AssistantChatMessage a partir de la respuesta recibida (completion).
            // Conserva sus datos, incluida la solicitud de Tool, en el formato que el SDK puede reenviar en el historial.
            // Lo guardamos en RawRepresentation para que se use al enviar la siguiente llamada al modelo.
            message.RawRepresentation = new OpenAI.Chat.AssistantChatMessage(completion);
        }
    }
}
