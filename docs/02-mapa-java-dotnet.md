# 02 - Mapa conceptual Java / .NET

La equivalencia entre ambos proyectos será conceptual y no necesariamente de API.

| Java / LangChain4j | .NET |
|---|---|
| Java | C# |
| Maven | dotnet CLI / NuGet |
| LangChain4j | Microsoft.Extensions.AI + Semantic Kernel |
| Chat Model | IChatClient / servicios de chat |
| Chat Memory | Chat history / estrategia de memoria |
| AI Services | servicios .NET + DI / abstracciones de mayor nivel |
| Prompt Templates | prompt templates |
| Structured Output | salida estructurada a tipos C# |
| Embeddings | IEmbeddingGenerator |
| Embedding Store | Vector Store |
| RAG | RAG |
| @Tool | Kernel Function / Function Calling |
| Agents | Semantic Kernel / Microsoft Agent Framework |
| Workflows | Agent Framework / orquestación |
| MCP | MCP |

## Regla

Cuando exista más de una alternativa .NET se elegirá la que mejor explique el concepto del laboratorio y no necesariamente la que permita escribir menos código.
