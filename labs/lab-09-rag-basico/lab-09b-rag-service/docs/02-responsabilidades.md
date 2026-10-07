# Lectura opcional - Responsabilidades

| Pieza | Transformación o tarea |
|---|---|
| `Program.cs` | Componer dependencias, ejecutar el ejemplo y presentar resultados |
| `RagService` | Coordinar indexación y consulta |
| `DocumentLoader` | archivo → texto |
| `TextChunker` | texto normalizado → chunks |
| `IEmbeddingGenerator` | texto → vector |
| `InMemoryVectorStore` | Almacenar asociaciones y buscar por similitud |
| `PromptTemplates` | contexto + pregunta → prompt |
| `IChatClient` | prompt → respuesta |

`IndexDocumentAsync` utiliza lectura, chunking y embeddings para cargar el índice. `AskAsync` utiliza embeddings, búsqueda, contexto y generación para responder.

El servicio no presenta resultados. Devuelve `RagResponse`, con `Answer` y `Sources`, para que `Program.cs` muestre tanto la respuesta como los resultados de recuperación.

Configuración y factories permanecen fuera del servicio. El store guarda el estado del índice; el servicio coordina operaciones sobre sus dependencias. No necesitamos otra capa de orquestación ni una clase para cada paso.

[Volver al laboratorio](../README.md).
