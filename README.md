# DAC Learning AI .NET

Laboratorio incremental para aprender desarrollo de aplicaciones con LLMs en .NET.

Este repositorio propone un recorrido incremental y autónomo para aprender integración de modelos de lenguaje utilizando herramientas naturales del ecosistema .NET.

## Objetivo

Aprender de forma incremental los conceptos principales necesarios para integrar modelos de lenguaje en aplicaciones .NET:

- acceso a un LLM;
- prompts;
- historial y memoria conversacional;
- salida estructurada;
- embeddings;
- carga y fragmentación de documentos;
- RAG;
- tools y function calling;
- agentes;
- workflows;
- MCP.

El foco está en los conceptos. Las APIs concretas pueden evolucionar.

## Stack previsto

- .NET 10 LTS
- C#
- Visual Studio Code Insiders
- Microsoft.Extensions.AI
- Semantic Kernel, cuando aporte valor al recorrido
- Microsoft Agent Framework, cuando corresponda
- OpenAI, Gemini, OpenRouter u otro proveedor compatible según cada laboratorio

## Estructura

```text
dac-learning-ai-dotnet/
├── .vscode/
├── docs/
├── labs/
│   ├── lab-00-introduccion/
│   ├── lab-01-hello-llm/
│   ├── ...
│   └── lab-17-final-project/
├── .editorconfig
├── .env.example
├── .gitignore
├── LICENSE
├── README.md
└── ROADMAP.md
```

Cada laboratorio tendrá su propio `README.md` y será autocontenido.

## Lab 00 - Rampa de entrada opcional

Antes del recorrido formal existe un Lab 00 opcional.

Su objetivo es permitir que alguien ejecute su **primera llamada a un modelo con muy pocas líneas de código**, antes de introducir configuración, helpers o estructura adicional.

Incluye dos ejemplos ejecutables:

- OpenAI;
- Gemini.

También contiene documentación breve sobre los artefactos que aparecerán durante la etapa **Fundamentos**.

Si ya conocés esas piezas, podés comenzar directamente por Lab 01.

## Estado actual

- ✅ Lab 00 - Introducción: implementado
- ✅ Lab 01 - Hello LLM: implementado
- ✅ Lab 02 - Chat History: implementado
- ✅ Lab 03 - Services y Dependency Injection: implementado
- ✅ Lab 04 - Prompt Templates: implementado
- ✅ Lab 05 - Structured Output: implementado
- ✅ Lab 06 - Chat Memory Advanced: implementado
- ✅ Lab 07 - Embeddings: implementado
- ⏳ Labs 08-17: pendientes de implementación

## Configuración común mediante `.env`

El repositorio utiliza **un único archivo `.env` en la raíz** para centralizar la configuración activa de los laboratorios.

Lab 00 es la única excepción: sus dos ejemplos mínimos llevan un placeholder de API key directamente en el código para reducir al máximo la fricción inicial.

A partir del Lab 01 la convención es:

```env
AI_PROVIDER=gemini
AI_MODEL=gemini-3.5-flash-lite
AI_API_KEY=tu-api-key
AI_URL=https://generativelanguage.googleapis.com/v1beta/openai/
```

### Variables

| Variable | Requerida | Uso |
|---|---:|---|
| `AI_PROVIDER` | Sí | identifica el proveedor activo |
| `AI_MODEL` | Sí | define explícitamente el modelo |
| `AI_API_KEY` | Sí* | credencial del proveedor remoto |
| `AI_URL` | Condicional | endpoint alternativo; puede quedar vacío cuando se usa el endpoint por defecto |

`*` Para los proveedores remotos utilizados actualmente por el taller.

La intención es evitar defaults y lógica de selección de proveedor dentro de cada `Program.cs`.

El código del laboratorio debería concentrarse en el concepto que se está estudiando.

### Ejemplos de configuración

#### Gemini

```env
AI_PROVIDER=gemini
AI_MODEL=gemini-3.5-flash-lite
AI_API_KEY=tu-api-key
AI_URL=https://generativelanguage.googleapis.com/v1beta/openai/
```

#### OpenAI

```env
AI_PROVIDER=openai
AI_MODEL=gpt-4o-mini
AI_API_KEY=tu-api-key
AI_URL=
```

#### OpenRouter

```env
AI_PROVIDER=openrouter
AI_MODEL=openrouter/free
AI_API_KEY=tu-api-key
AI_URL=https://openrouter.ai/api/v1
```

`.env` contiene valores reales y **no debe versionarse**.

`.env.example` documenta la estructura y **sí debe versionarse**.

## Principio de diseño de los laboratorios

Una crítica útil durante la evolución del proyecto fue que código necesario para configurar proveedores podía ocultar el concepto pedagógico principal.

A partir de esta revisión distinguimos explícitamente:

```text
Código necesario para que el laboratorio funcione
                    vs
Código necesario para comprender el concepto
```

La infraestructura repetitiva —carga de configuración, endpoint, credenciales y construcción del cliente— debe quedar encapsulada cuando no sea el objeto de estudio.

El `Program.cs` debería permitir identificar rápidamente qué concepto introduce ese laboratorio.

## Acceso a los modelos

El proyecto procura no quedar acoplado a un único proveedor.

La selección se describe mediante una configuración uniforme y las abstracciones de `Microsoft.Extensions.AI` permiten mantener el foco en conceptos como:

```text
IChatClient
ChatMessage
ChatResponse
IEmbeddingGenerator
```

Los modelos, planes y límites de cada proveedor pueden cambiar. Para los laboratorios deben utilizarse únicamente datos ficticios o públicos.

## Documentación

- [`ROADMAP.md`](ROADMAP.md) — visión global, hitos y estado de evolución del proyecto.
- [`labs/lab-00-introduccion/README.md`](labs/lab-00-introduccion/README.md) — rampa de entrada y mapa de artefactos de Fundamentos.
- `docs/00-vision-y-alcance.md`
- `docs/01-entorno-vscode-insiders.md`
- `docs/03-roadmap-labs.md` — recorrido pedagógico detallado de los laboratorios.

## Criterio pedagógico

El proyecto está pensado como una progresión propia de .NET.

```text
Lab 00  primera ejecución + mapa de artefactos (opcional)
   ↓
Lab 01  IChatClient + primera llamada formal
   ↓
Lab 02  historial conversacional
   ↓
Lab 03  servicios + Dependency Injection
   ↓
Lab 04  Prompt Templates
   ↓
Lab 05  Structured Output
   ↓
Lab 06  memoria por sesión
   ↓
Lab 07  embeddings + similitud semántica
```

La prioridad es comprender primero el concepto y recién después incorporar abstracciones de mayor nivel.

Los frameworks y APIs concretas son herramientas del recorrido, no el objetivo final del aprendizaje.
