# Metadata nativa y compatibilidad del adaptador

Automatizar un protocolo y transportar correctamente los datos de un proveedor son responsabilidades relacionadas, pero distintas.

## Lo que comprobamos primero

La primera ejecución de 11b utilizó `UseFunctionInvocation()` sin preservar la representación nativa. El cálculo se ejecutó y devolvió `1210`, pero Gemini rechazó la continuación con HTTP 400 por falta de `thought_signature`.

El cliente de invocación conservaba el historial necesario. Al continuar, `OpenAIChatClient` reconstruía un mensaje desde la representación común y no recuperaba la extensión presente en la respuesta nativa. El contrato común no necesita incluir cada campo específico del proveedor; la respuesta original seguía accesible mediante `RawRepresentation`.

La [implementación del adaptador utilizada](https://github.com/dotnet/extensions/blob/v10.10.1/src/Libraries/Microsoft.Extensions.AI.OpenAI/OpenAIChatClient.cs) permite reenviar directamente un mensaje nativo cuando lo encuentra en `ChatMessage.RawRepresentation`. El SDK también permite crear un `AssistantChatMessage` desde el `ChatCompletion` original.

## Ubicación del ajuste

```text
Program.cs
    ↓
FunctionInvokingChatClient
    ↓
cliente base de ChatClientFactory
    ↓ conserva el mensaje nativo recibido
adaptador OpenAI-compatible
    ↓
proveedor
```

Dentro de la factory usamos `ChatClientBuilder.Use` para conservar el mensaje nativo en cada respuesta. El adaptador coloca inicialmente un `ChatCompletion` en `ChatMessage.RawRepresentation`; lo convertimos en un `AssistantChatMessage` nativo que el mismo adaptador puede reutilizar al enviar el historial. `RawRepresentation` contiene un objeto específico del SDK, no necesariamente JSON HTTP crudo. La API real requiere indicar ambos argumentos del hook; utilizamos `getResponseFunc` y pasamos `getStreamingResponseFunc: null` porque el ejercicio no usa streaming.

El ajuste no busca Function Calls, no ejecuta Tools y no construye Function Results. Tampoco comprueba `AI_PROVIDER`: actúa sobre respuestas del SDK utilizado. Por eso el flujo de 11b permanece centrado en definir una Tool y consultar al cliente de invocación automática.

## Qué se conserva

`new OpenAI.Chat.AssistantChatMessage(completion)` construye el mensaje nativo directamente desde la respuesta original. Conservamos información ya recibida, incluida la firma del caso observado. No inventamos metadata, no interpretamos `thought_signature` ni generamos firmas. El requisito de Google, también para el endpoint compatible con OpenAI, está en su [documentación oficial de thought signatures](https://ai.google.dev/gemini-api/docs/generate-content/thought-signatures).

Después de preservar el mensaje, la prueba real con el mismo Gemini completó la respuesta final. La implementación pertenece a la integración basada en el SDK OpenAI, incluidos endpoints OpenAI-compatible que utilicen ese SDK. Un proveedor integrado mediante otro SDK puede exponer otra representación nativa y necesitar otro tratamiento. Tampoco garantizamos que todas las extensiones se conserven con cualquier versión: comprobamos la combinación concreta de modelo, endpoint y adaptador.

## Comparación con 11a

11a muestra la gestión explícita de Function Call, argumentos, ejecución, Function Result, historial y continuación. Allí también conserva la representación nativa comprobando su tipo, sin condiciones por proveedor.

11b delega el ciclo a `FunctionInvokingChatClient` y conserva la representación nativa en la capa del `IChatClient`. Vemos el mecanismo en 11a y luego ubicamos sus responsabilidades en las abstracciones y capas apropiadas en 11b.

La [sección conceptual de 11a](../../lab-11a-tool-explicita/README.md#chatresponse-y-la-representación-nativa) explica `ChatResponse`, `ChatMessage` y `AIContent`. El principio es programar contra el modelo común y conservar la información nativa cuando el protocolo la necesita. Gemini y `thought_signature` son el caso que lo hizo visible.

[Volver a 11b](../README.md).
