# Lectura opcional - `ChatClientFactory`

## Objetivo

`ChatClientFactory` transforma una configuración válida en:

```csharp
IChatClient
```

y evita que `Program.cs` tenga que conocer los detalles de creación del cliente.

---

## `Create()`

```csharp
public static IChatClient Create(
    LabConfiguration configuration)
```

recibe:

```text
modelo
API key
endpoint opcional
```

y devuelve siempre:

```text
IChatClient
```

---

## Sin endpoint alternativo

Si:

```csharp
configuration.Endpoint is null
```

se crea directamente:

```csharp
new ChatClient(
    configuration.Model,
    configuration.ApiKey)
```

y luego se adapta mediante:

```csharp
.AsIChatClient()
```

---

## Con endpoint alternativo

Si `AI_URL` tiene valor, se construye:

```csharp
OpenAIClientOptions
```

con ese endpoint.

Esto permite utilizar proveedores que exponen una API compatible con el cliente utilizado por el laboratorio.

---

## Idea principal

Los distintos caminos de configuración terminan en la misma abstracción:

```text
configuración
     ↓
ChatClientFactory
     ↓
IChatClient
```

El Lab 02 puede entonces concentrarse exclusivamente en:

```text
mensajes
historial
contexto
```
