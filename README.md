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
- Semantic Kernel
- Microsoft Agent Framework, cuando corresponda
- OpenAI, Gemini, OpenRouter u otro proveedor compatible según cada laboratorio

## Estructura

```text
dac-learning-ai-dotnet/
├── .vscode/
├── docs/
├── labs/
├── .editorconfig
├── .gitignore
├── LICENSE
├── README.md
└── ROADMAP.md
```

Cada laboratorio tendrá su propio `README.md` y será autocontenido.

## Estado actual

- ✅ Lab 01 - Hello LLM: implementado
- ✅ Lab 02 - Chat History: implementado
- ✅ Lab 03 - Services y Dependency Injection: implementado
- ✅ Lab 04 - Prompt Templates: implementado
- ⏳ Labs 05-17: estructura preparada, pendientes de implementación


## Configuración común mediante `.env`

El repositorio utiliza **un único archivo `.env` en la raíz** para centralizar la configuración común de todos los laboratorios.

```text
dac-learning-ai-dotnet/
├── .env
├── .env.example
├── README.md
├── ROADMAP.md
├── docs/
└── labs/
    ├── lab-01-hello-llm/
    ├── lab-02-chat-history/
    └── ...
```

No es necesario crear un `.env` dentro de cada laboratorio.

`.env` contiene los valores reales de cada desarrollador y **no debe versionarse**.

`.env.example` documenta las variables necesarias y **sí debe versionarse**.

Ejemplo:

```env
AI_PROVIDER=gemini
AI_MODEL=

OPENAI_API_KEY=
GEMINI_API_KEY=
OPENROUTER_API_KEY=
```

Cada laboratorio que necesite configuración deberá reutilizar este archivo común ubicado en la raíz.

## Acceso a los modelos

El proyecto busca que el costo de una API no sea una barrera para aprender.

Los laboratorios implementados admiten actualmente:

- **OpenAI**;
- **Gemini**, como alternativa online con nivel gratuito;
- **OpenRouter**, incluyendo `openrouter/free`.

La selección se realiza mediante `AI_PROVIDER`, manteniendo `IChatClient` como abstracción común.

Los planes gratuitos, modelos y límites dependen de cada proveedor y pueden cambiar. Para los laboratorios deben utilizarse únicamente datos ficticios o públicos.

## Documentación

- [`ROADMAP.md`](ROADMAP.md) — visión global, hitos y estado de evolución del proyecto.
- `docs/00-vision-y-alcance.md`
- `docs/01-entorno-vscode-insiders.md`
- `docs/02-mapa-java-dotnet.md`
- `docs/03-roadmap-labs.md` — recorrido pedagógico detallado de los laboratorios.

## Criterio pedagógico

El proyecto está pensado como una progresión propia de .NET.

Cada laboratorio introduce una idea principal y reutiliza lo aprendido anteriormente:

```text
Lab 01
IChatClient + primera llamada
        ↓
Lab 02
historial conversacional
        ↓
Lab 03
servicios + Dependency Injection
        ↓
Lab 04
Prompt Templates
```

La prioridad es comprender primero el concepto y recién después incorporar abstracciones de mayor nivel.

Los frameworks y APIs concretas son herramientas del recorrido, no el objetivo final del aprendizaje.
