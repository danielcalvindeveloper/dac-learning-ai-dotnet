# Lectura opcional - Configuración y `AiClientFactory`

## Continuidad con Lab 03

El Lab 04 no introduce un nuevo mecanismo de configuración.

Reutiliza exactamente:

```text
LabConfiguration
        ↓
AiClientFactory
        ↓
IChatClient
```

Esto es importante porque el foco del laboratorio son los Prompt Templates.

---

## `LabConfiguration`

Carga desde el `.env`:

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
AI_URL
```

Las primeras tres variables son obligatorias.

`AI_URL` es opcional.

También valida que, si existe, sea una URL absoluta válida.

---

## `AiClientFactory`

Recibe indirectamente esa configuración mediante:

```csharp
LabConfiguration.Load()
```

y construye:

```csharp
IChatClient
```

Puede utilizar:

```text
endpoint estándar
```

o:

```text
AI_URL
```

si está configurada.

---

## Qué no hace

`AiClientFactory` no:

- construye prompts;
- conoce `PromptTemplates`;
- llama a `Assistant`;
- imprime resultados;
- implementa operaciones de negocio.

---

## Relación con DI

`Program.cs` registra:

```csharp
services.AddSingleton<IChatClient>(
    _ => AiClientFactory.CreateFromEnvironment());
```

El contenedor crea ese cliente cuando necesita construir `Assistant`.

Así se conserva la separación:

```text
infraestructura
    LabConfiguration
    AiClientFactory

servicio
    IAssistant
    Assistant

prompts
    PromptTemplates
```

---

## Idea principal

La configuración ya es un problema resuelto por los laboratorios anteriores.

En Lab 04 simplemente la reutilizamos.
