# El ciclo conversacional de Function Calling

Function Calling necesita un intercambio de mensajes. El historial permite relacionar la pregunta, la función solicitada y el resultado de la aplicación.

## Conservar la solicitud

Después de la primera llamada, guardamos los mensajes completos que expone la abstracción:

```csharp
ChatResponse response = await chatClient.GetResponseAsync(messages, options);
// Program.cs conserva aquí la representación nativa antes de guardar.
messages.AddRange(response.Messages);
```

`response.Text` por sí solo no alcanza. La solicitud está en `FunctionCallContent`, dentro de `ChatMessage.Contents`. Recorremos los contenidos de `response.Messages` y buscamos ese tipo.

La aplicación comprueba que haya una única solicitud a la función conocida. Si no la hay, termina informándolo. No fabrica una solicitud ni ejecuta la función por su cuenta para aparentar un ciclo exitoso.

## Relacionar solicitud y resultado

Luego de ejecutar el método, construimos el resultado:

```csharp
FunctionResultContent functionResult = new(functionCall.CallId, result);
messages.Add(new ChatMessage(ChatRole.Tool, [functionResult]));
```

`CallId` identifica esa llamada, no la función en general. Copiarlo desde la solicitud permite asociar el resultado con la operación pedida. No debemos inventar otro identificador.

El resultado tiene rol `Tool`. No es una nueva pregunta del usuario ni una respuesta natural del asistente.

## Segunda llamada

```csharp
finalResponse = await chatClient.GetResponseAsync(messages);
```

Enviamos los mensajes iniciales, los mensajes de respuesta del modelo y el resultado de la Tool. Esta llamada no ofrece nuevas herramientas: esperamos la respuesta final para una única operación.

El programa imprime `finalResponse.Text`. El resultado numérico proviene de C#; el modelo produce la redacción natural. Son responsabilidades diferentes.

## Límite del ejemplo

Son dos llamadas de chat coordinadas explícitamente. No hay `FunctionInvokingChatClient`, `UseFunctionInvocation()`, un dispatcher ni un bucle genérico. [Lab 11b](../../lab-11b-tool-invocation/README.md) delega este mismo ciclo a `FunctionInvokingChatClient`. Lab 12 estudiará después varias Tools.

Como administramos explícitamente la continuación, también debemos conservar la representación nativa necesaria. Antes de guardar los mensajes, `Program.cs` comprueba si cada `ChatMessage.RawRepresentation` contiene un `OpenAI.Chat.ChatCompletion` y construye su `AssistantChatMessage` nativo. El adaptador puede reenviarlo sin reconstruirlo sólo desde los contenidos comunes. No se consulta `AI_PROVIDER`.

Gemini y `thought_signature` fueron el caso real que permitió observar esta necesidad. La solución depende de la representación del SDK OpenAI, no del nombre del proveedor. `RawRepresentation` contiene aquí un objeto del SDK, no necesariamente JSON HTTP crudo. La [explicación conceptual](../README.md#chatresponse-y-la-representación-nativa) y la [validación real](../README.md#validación-y-compatibilidad) amplían ese punto.

[Volver al laboratorio](../README.md).
