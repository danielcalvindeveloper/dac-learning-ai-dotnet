# Lectura opcional - Configuración y `AiClientFactory`

## Objetivo

Estas clases existen para mantener fuera del flujo principal la infraestructura necesaria para crear:

```csharp
IChatClient
```

No forman parte del concepto de servicio de aplicación.

---

# `LabConfiguration`

Carga:

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
AI_URL
```

desde el `.env` común.

Las primeras tres variables son obligatorias.

`AI_URL` es opcional.

---

## Validación de `AI_URL`

Si está vacía:

```text
Endpoint = null
```

Si tiene contenido, se valida como URL absoluta.

Una URL inválida produce:

```csharp
InvalidOperationException
```

igual que una variable obligatoria ausente.

---

# `AiClientFactory`

Su método:

```csharp
CreateFromEnvironment()
```

realiza este flujo:

```text
LabConfiguration.Load()
        ↓
obtener configuración válida
        ↓
crear cliente concreto
        ↓
AsIChatClient()
        ↓
IChatClient
```

---

## ¿Qué no hace?

La Factory no:

- implementa chat;
- traduce;
- resume;
- decide prompts;
- imprime resultados del ejercicio.

Es infraestructura.

---

## Endpoint estándar

Cuando:

```csharp
configuration.Endpoint is null
```

el cliente usa su endpoint normal.

---

## Endpoint alternativo

Cuando `AI_URL` está configurada:

```csharp
OpenAIClientOptions options = new()
{
    Endpoint = configuration.Endpoint
};
```

Esto permite reutilizar el mismo tipo de cliente con un endpoint compatible.

---

## Separación de responsabilidades

```text
LabConfiguration
    obtiene datos

AiClientFactory
    construye IChatClient

Assistant
    usa IChatClient

Program
    usa IAssistant
```

Esa separación permite que el lector del Lab 03 pueda concentrarse primero en los servicios.
