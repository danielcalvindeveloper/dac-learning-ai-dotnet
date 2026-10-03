# IChatClient y los clientes concretos

## El problema

Una aplicación necesita alguna pieza que conozca el protocolo real del proveedor.

Por ejemplo:

```csharp
OpenAI.Chat.ChatClient
```

Ese cliente sabe comunicarse con un servicio compatible.

Pero no queremos que toda nuestra lógica de aplicación dependa directamente de esa clase.

## IChatClient

`IChatClient` representa el contrato que necesita la aplicación para conversar con un modelo.

```text
Aplicación
    ↓
IChatClient
    ↓
cliente concreto
    ↓
servicio remoto
```

## AsIChatClient

El método:

```csharp
AsIChatClient()
```

adapta un cliente concreto a `IChatClient`.

Ejemplo:

```csharp
IChatClient chatClient =
    openAiClient.AsIChatClient();
```

## GetResponseAsync

La operación básica que utilizaremos es:

```csharp
await chatClient.GetResponseAsync(prompt);
```

Recibe un mensaje o una colección de mensajes y devuelve un `ChatResponse`.

## Idea principal

El resto del taller puede concentrarse en:

```text
mensaje → IChatClient → respuesta
```

mientras la configuración y el proveedor concreto quedan fuera del foco del ejercicio.
