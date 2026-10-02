# Lab 01 - Hello LLM

## Objetivo

Realizar la primera llamada a un modelo de lenguaje desde una aplicación de consola .NET.

El laboratorio busca responder una pregunta muy simple:

> ¿Cuál es el mínimo necesario para enviar un prompt a un LLM desde C# y obtener una respuesta?

No se utilizarán todavía memoria, inyección de dependencias, Semantic Kernel, tools, agentes ni RAG.

---

## Conceptos que aparecen

- aplicación de consola .NET;
- paquete NuGet;
- API key;
- proveedor de modelos;
- modelo de lenguaje;
- prompt;
- `IChatClient`;
- respuesta del modelo.

## Stack

- .NET 10
- C#
- Visual Studio Code Insiders
- `Microsoft.Extensions.AI.OpenAI`
- OpenAI, Gemini u OpenRouter

La aplicación utiliza `IChatClient`, la abstracción de `Microsoft.Extensions.AI` para interactuar con servicios de chat generativo.

El objetivo pedagógico no cambia según el proveedor utilizado.

---

## Accesibilidad económica

**No es obligatorio disponer de una cuenta paga de OpenAI para realizar este laboratorio.**

El ejemplo admite tres proveedores:

| `AI_PROVIDER` | Uso previsto | Modelo por defecto |
|---|---|---|
| `openai` | opción original | `gpt-4o-mini` |
| `gemini` | alternativa online con nivel gratuito | `gemini-3.8-flash` |
| `openrouter` | alternativa online gratuita | `openrouter/free` |

Los niveles gratuitos, modelos disponibles y límites pertenecen a cada proveedor y pueden cambiar con el tiempo.

La intención es evitar que el costo de una API sea una barrera para realizar los laboratorios.

> Los ejemplos de aprendizaje no deben utilizar datos personales, confidenciales ni información sensible.

---

## ¿Por qué podemos mantener el mismo código?

Gemini y OpenRouter disponen de endpoints compatibles con la API de OpenAI.

Esto permite conservar la misma estructura:

```text
Aplicación
    │
    ▼
IChatClient
    │
    ├── OpenAI
    ├── Gemini
    └── OpenRouter
```

y cambiar fundamentalmente:

- API key;
- endpoint;
- modelo.

---

## Estructura

```text
lab-01-hello-llm/
├── Lab01.HelloLlm.csproj
├── Program.cs
└── README.md
```

## 1. Abrir el laboratorio

Desde la raíz del repositorio:

```powershell
cd labs\lab-01-hello-llm
code-insiders .
```

## 2. Verificar .NET

```powershell
dotnet --version
```

El proyecto está configurado para:

```text
net10.0
```

## 3. Restaurar dependencias

```powershell
dotnet restore
```

El laboratorio mantiene una única dependencia explícita:

```xml
<PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="10.10.1" />
```

No es necesario agregar un paquete diferente para Gemini u OpenRouter en este laboratorio.

---


## Ubicación del `.env`

El laboratorio **no utiliza un `.env` propio**.

La configuración se comparte con el resto del proyecto mediante un único archivo ubicado en la raíz:

```text
dac-learning-ai-dotnet/
├── .env
├── .env.example
└── labs/
    └── lab-01-hello-llm/
        ├── Lab01.HelloLlm.csproj
        └── Program.cs
```

Esto permite que todos los laboratorios reutilicen las mismas API keys y la misma selección de proveedor.

`.env` no debe subirse a Git.

`.env.example` sí debe formar parte del repositorio porque documenta las variables necesarias.

Cuando el Lab 01 incorpore la carga automática de configuración, buscará este `.env` común desde la carpeta del laboratorio hacia la raíz.

---

## 4. Elegir proveedor

La variable:

```text
AI_PROVIDER
```

puede tomar:

```text
openai
gemini
openrouter
```

Si no se define, se utiliza `openai`.

### Opción A — OpenAI

```powershell
$env:AI_PROVIDER="openai"
$env:OPENAI_API_KEY="tu-api-key"
```

Modelo por defecto:

```text
gpt-4o-mini
```

### Opción B — Gemini

```powershell
$env:AI_PROVIDER="gemini"
$env:GEMINI_API_KEY="tu-api-key"
```

Modelo por defecto del laboratorio:

```text
gemini-3.8-flash
```

Gemini dispone de nivel gratuito para determinados modelos. La API key puede obtenerse desde Google AI Studio.

### Opción C — OpenRouter

```powershell
$env:AI_PROVIDER="openrouter"
$env:OPENROUTER_API_KEY="tu-api-key"
```

Modelo por defecto:

```text
openrouter/free
```

`openrouter/free` selecciona automáticamente un modelo gratuito disponible compatible con la solicitud.

---

## Seleccionar otro modelo

Independientemente del proveedor puede sobrescribirse el modelo:

```powershell
$env:AI_MODEL="nombre-del-modelo"
```

Ejemplo:

```powershell
$env:AI_PROVIDER="gemini"
$env:GEMINI_API_KEY="tu-api-key"
$env:AI_MODEL="gemini-3.8-flash"
```

---

## 5. Ejecutar

```powershell
dotnet run
```

Salida aproximada:

```text
Proveedor: Gemini
Modelo: gemini-3.8-flash
Prompt: Explica en una sola oración qué es un modelo de lenguaje.

Respuesta:
Un modelo de lenguaje es ...
```

La respuesta exacta puede variar entre ejecuciones y proveedores.

---

## Código principal

La aplicación sigue trabajando con:

```csharp
IChatClient chatClient
```

La selección de proveedor ocurre antes de construir el cliente.

Para mantener el código principal simple, el detalle de cada proveedor queda encapsulado en:

```csharp
ResolveProviderConfiguration(provider)
```

Ese método resuelve únicamente lo necesario para conectarse:

- variable de API key;
- modelo por defecto;
- endpoint.

La interacción posterior sigue siendo idéntica:

```csharp
ChatResponse response = await chatClient.GetResponseAsync(prompt);

Console.WriteLine(response.Text);
```

---

## Flujo

```text
             AI_PROVIDER
                  │
        ┌─────────┼─────────┐
        ▼         ▼         ▼
     OpenAI    Gemini   OpenRouter
        │         │         │
        └─────────┼─────────┘
                  ▼
             IChatClient
                  │
                  ▼
               Prompt
                  │
                  ▼
               Modelo
                  │
                  ▼
            ChatResponse
```

---

## ¿Por qué `IChatClient`?

Podríamos utilizar directamente el cliente específico de cada proveedor, pero eso mezclaría desde el primer laboratorio el concepto de LLM con una implementación concreta.

`IChatClient` permite separar ambos conceptos.

En este laboratorio esa ventaja ya puede observarse de forma concreta: el código consumidor no necesita saber si la respuesta proviene de OpenAI, Gemini u OpenRouter.

---

## Relación con LangChain4j

La equivalencia conceptual es aproximadamente:

```text
LangChain4j                    .NET
────────────────────────────────────────
ChatLanguageModel       ↔      IChatClient
OpenAiChatModel         ↔      OpenAI ChatClient
model.generate(...)     ↔      GetResponseAsync(...)
respuesta String        ↔      ChatResponse.Text
```

No se busca una traducción API por API. Se busca resolver el mismo problema utilizando la forma natural del ecosistema .NET.

---

## Qué NO hace todavía este laboratorio

El programa no recuerda conversaciones.

Cada ejecución es:

```text
prompt → modelo → respuesta
```

No existe todavía:

```text
mensaje 1
   ↓
mensaje 2
   ↓
contexto de conversación
```

Ese será el objetivo del **Lab 02 - Chat History**.

Tampoco se pretende estudiar todavía diferencias entre proveedores. La posibilidad de elegir uno existe únicamente para facilitar el acceso al laboratorio.

---

## Resultado esperado

Al finalizar el laboratorio deberías poder explicar:

1. qué representa `IChatClient`;
2. cómo se elige un proveedor;
3. dónde se configura la API key;
4. cómo se selecciona el modelo;
5. cómo se envía un prompt;
6. cómo se obtiene el texto de la respuesta;
7. por qué el concepto aprendido no depende de OpenAI.
