# DAC Learning AI .NET

Laboratorio incremental para aprender desarrollo de aplicaciones con LLMs en .NET.

Este repositorio nace como proyecto paralelo de `dac-learning-langchain4j`, manteniendo una progresión conceptual equivalente pero utilizando herramientas naturales del ecosistema .NET.

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
- ⏳ Labs 02-17: estructura preparada, pendientes de implementación

## Acceso a los modelos

El proyecto busca que el costo de una API no sea una barrera para aprender.

El Lab 01 admite actualmente:

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

La versión .NET no intentará traducir las APIs de LangChain4j línea por línea.

La correspondencia será conceptual:

```text
Concepto Java/LangChain4j
        ↓
Mismo problema
        ↓
Solución idiomática en .NET
```

De esta forma ambos repositorios pueden recorrerse en paralelo sin convertir uno en una copia artificial del otro.
