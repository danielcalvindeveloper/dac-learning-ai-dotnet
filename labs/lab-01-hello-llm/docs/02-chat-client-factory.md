# Lectura opcional - `ChatClientFactory`

## ¿Por qué existe?

`ChatClientFactory` construye el objeto que finalmente utilizará el laboratorio:

```csharp
IChatClient
```

Su objetivo es esconder del `Program.cs` los detalles necesarios para crear el cliente.

---

## Método `Create()`

```csharp
public static IChatClient Create(
    LabConfiguration configuration)
```

recibe una configuración ya validada y devuelve:

```csharp
IChatClient
```

Eso significa que el resto de la aplicación no necesita conocer el tipo concreto que se creó.

---

## Caso sin `AI_URL`

Cuando:

```csharp
configuration.Endpoint is null
```

se crea directamente:

```csharp
new ChatClient(
    configuration.Model,
    configuration.ApiKey)
```

Este es el caso típico de OpenAI.

Después:

```csharp
.AsIChatClient()
```

adapta el cliente concreto de OpenAI a la abstracción:

```csharp
IChatClient
```

---

## Caso con `AI_URL`

Algunos proveedores exponen un endpoint compatible con la API utilizada por el cliente OpenAI.

En ese caso primero se configura:

```csharp
OpenAIClientOptions options = new()
{
    Endpoint = configuration.Endpoint
};
```

y luego se construye:

```csharp
ChatClient client = new(
    configuration.Model,
    new ApiKeyCredential(configuration.ApiKey),
    options);
```

El resultado también se adapta:

```csharp
client.AsIChatClient();
```

---

## El punto importante

Los dos caminos terminan en el mismo tipo:

```text
OpenAI sin endpoint alternativo ─┐
                                ├──> IChatClient
Proveedor con AI_URL ───────────┘
```

A partir de ese momento el código consumidor puede ignorar cómo fue creado.

---

## ¿Por qué una Factory?

Podríamos escribir este código directamente en `Program.cs`.

La Factory no es necesaria para llamar a un LLM.

Está aquí para que el flujo principal pueda decir simplemente:

```csharp
IChatClient chatClient =
    ChatClientFactory.Create(configuration);
```

y continuar con el concepto del laboratorio.

---

## Idea principal

`ChatClientFactory` resuelve:

```text
configuración
      ↓
cliente concreto
      ↓
IChatClient
```

No contiene lógica del ejercicio; prepara la dependencia que el ejercicio necesita.
