# Lectura opcional - `LabConfiguration`

## ¿Por qué existe?

`LabConfiguration` concentra la lectura y validación de la configuración necesaria para conectarse a un proveedor.

Sin esta clase, `Program.cs` tendría que leer:

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
AI_URL
```

y validar cada valor antes de llegar al ejemplo que queremos estudiar.

Su objetivo es retirar ese ruido del flujo principal.

---

## El `record`

```csharp
internal sealed record LabConfiguration(
    string ProviderName,
    string ApiKey,
    string Model,
    Uri? Endpoint)
```

representa una configuración ya cargada y lista para utilizar.

Contiene cuatro datos:

| Propiedad | Objetivo |
|---|---|
| `ProviderName` | Nombre descriptivo del proveedor |
| `ApiKey` | Credencial |
| `Model` | Modelo elegido |
| `Endpoint` | URL alternativa, si existe |

---

## `Load()`

```csharp
public static LabConfiguration Load()
```

es el punto de entrada.

Primero ejecuta:

```csharp
Env.TraversePath().Load();
```

Esto busca el archivo `.env` recorriendo los directorios padres.

Es útil porque el `.env` está en la raíz del repositorio y el programa se ejecuta desde:

```text
labs/lab-01-hello-llm
```

Después obtiene las variables obligatorias:

```csharp
string provider = GetRequired("AI_PROVIDER");
string model = GetRequired("AI_MODEL");
string apiKey = GetRequired("AI_API_KEY");
```

Finalmente lee:

```csharp
AI_URL
```

como valor opcional.

Si existe, se transforma en:

```csharp
Uri
```

Si está vacío:

```csharp
Endpoint = null
```

---

## `GetRequired()`

```csharp
private static string GetRequired(string variableName)
```

evita repetir la misma validación para cada variable obligatoria.

Su responsabilidad es:

```text
leer variable
    ↓
¿existe?
 ├─ sí → devolver valor
 └─ no → lanzar error claro
```

No contiene lógica relacionada con ningún proveedor concreto.

---

## ¿Qué simplificamos respecto de una configuración por proveedor?

No existen:

```text
DefaultOpenAiModel
DefaultGeminiModel
DefaultOpenRouterModel

OPENAI_API_KEY
GEMINI_API_KEY
OPENROUTER_API_KEY

ResolveProviderConfiguration(...)
```

Toda esa información queda externalizada.

La configuración activa es explícita en `.env`.

---

## Idea principal

`LabConfiguration` no enseña IA.

Es infraestructura mínima para permitir que el resto del laboratorio pueda concentrarse en IA.
