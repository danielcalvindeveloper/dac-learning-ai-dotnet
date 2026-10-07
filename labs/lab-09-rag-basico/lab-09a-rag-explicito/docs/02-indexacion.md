# Lectura opcional - Indexación

```text
documento → texto → chunks → embeddings → índice
```

Esta etapa prepara el contenido y puede realizarse antes de recibir preguntas. En el ejemplo vuelve a ejecutarse al iniciar el proceso porque el índice vive solamente en RAM.

`DocumentLoader` lee `.txt` o `.md`. `TextChunker` normaliza saltos de línea, aplica `Trim()` al documento y divide por caracteres, con tamaño 500 y overlap 100. No interpreta Markdown ni preserva perfectamente palabras u oraciones.

`GenerateAsync` recibe los contenidos de todos los chunks en una colección. Antes de asociar resultados, `Program.cs` comprueba que haya exactamente un embedding por chunk; una diferencia produce `InvalidOperationException`.

Cada `IndexedChunk` asocia un `DocumentChunk` con su `ReadOnlyMemory<float> Vector`. `DocumentChunk` conserva `Content`, `Source` e `Index`; el vector no reemplaza el texto que después necesitamos recuperar.

`InMemoryVectorStore.Add` incorpora esa asociación a una lista. No entrena un modelo ni construye una base externa. Los embeddings de chunks y pregunta deben generarse con el mismo modelo y configuración.

[Volver al laboratorio](../README.md).
