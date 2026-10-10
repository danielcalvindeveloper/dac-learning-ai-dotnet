# Qué coordina FunctionInvokingChatClient

En 11a coordinamos a mano el intercambio de solicitud y resultado. En 11b conservamos la función y delegamos esa responsabilidad a un componente existente del framework.

## Composición del cliente

```csharp
using IChatClient chatClient = new ChatClientBuilder(
    ChatClientFactory.Create(configuration))
    .UseFunctionInvocation()
    .Build();
```

`UseFunctionInvocation()` incorpora `FunctionInvokingChatClient`. `Build()` devuelve un `IChatClient`, por lo que la aplicación utiliza la misma abstracción de chat.

## Un llamado de aplicación, varios intercambios

Al recibir una consulta con `ChatOptions.Tools`, el cliente envía los mensajes al modelo. Si la respuesta solicita una `AIFunction` disponible, coordina la invocación, construye el resultado y continúa la conversación. El componente tiene límites de iteración; no es un bucle ilimitado.

En el caso observado hubo dos llamadas al proveedor: una para solicitar la Tool y otra para generar la respuesta a partir de su resultado. Ese detalle ya no se escribe en `Program.cs`.

## La ejecución sigue siendo local

La función que imprime `=== TOOL EJECUTADA POR .NET ===` sigue siendo nuestro código C#. El modelo aporta una solicitud con argumentos; la abstracción invoca el método dentro de la aplicación y transporta el resultado.

No cambia la responsabilidad del modelo ni convierte este ejemplo en un agente. La aplicación sigue controlando qué función ofrece y qué implementación tiene.

## Por qué después de 11a

Ahora podemos reconocer los pasos que encapsula el framework. 11a muestra el mecanismo deliberadamente; 11b permite utilizarlo sin repetir su coordinación en cada consulta.

La compatibilidad del cliente base se estudia por separado en [metadata y compatibilidad](02-metadata-y-compatibilidad.md).

[Volver a 11b](../README.md).
