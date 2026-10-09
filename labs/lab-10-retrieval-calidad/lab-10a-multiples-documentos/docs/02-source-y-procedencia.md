# Lectura opcional - Source y procedencia

Un resultado de búsqueda contiene texto, pero también necesitamos reconocer de dónde salió. `Source` conserva el nombre de archivo y `Index` la posición del chunk dentro de ese documento.

## Una posición local

Cada llamada a `TextChunker.Split` crea una secuencia nueva:

```text
embeddings.md: 0, 1, 2, ...
rag.md:        0, 1, 2, ...
```

Los índices repetidos son correctos. `rag.md - Chunk 0` y `embeddings.md - Chunk 0` identifican fragmentos distintos. El ejemplo usa nombres únicos en una carpeta plana; no introduce identificadores globales.

## Del índice al contexto

`IndexedChunk` asocia el `DocumentChunk` con su vector. `SearchResult` conserva ese chunk y agrega la similitud de la consulta. Por eso el programa puede mostrar fuente, posición y score después de buscar, aunque todos los documentos compartan el mismo índice.

El encabezado `[Fuente: <archivo> - Chunk <índice>]` acompaña al contenido cuando construimos el contexto. También mostramos esos datos en consola. Esto permite inspeccionar qué textos llegaron al modelo generativo.

La procedencia del material recuperado no valida automáticamente la respuesta. En este paso no hay citaciones formales ni comprobación de cada afirmación del modelo.

[Volver al laboratorio](../README.md).
