# Lectura opcional - Retrieval multifuente

La consulta fija relaciona embeddings y RAG. Esos conceptos aparecen en dos documentos del corpus, por lo que una respuesta puede apoyarse en fragmentos de ambas fuentes.

## Qué compara la búsqueda

```text
un embedding de pregunta
    ↓
comparación con todos los chunks indexados
    ↓
orden descendente de similitud
    ↓
hasta topK fragmentos
```

Se conserva `InMemoryVectorStore.Search` de Lab 09. La similitud coseno sigue siendo infraestructura conocida. No seleccionamos un archivo completo antes de comparar ni agrupamos resultados para dar lugar a cada fuente.

## Multifuente no significa reparto obligatorio

`topK = 3` puede devolver tres chunks de un mismo documento, dos de uno y uno de otro, o tres fuentes diferentes. Depende de su cercanía a la pregunta. El orden estable de archivos no impone diversidad ni garantiza el mismo ranking para cualquier modelo.

La consola presenta `Source`, `Index` y similitud, y luego el contexto completo. Esto permite observar qué información aportó cada documento a la llamada generativa.

## La pregunta que sigue

El store devuelve los mejores fragmentos disponibles aunque todos estén poco relacionados. El número de resultados es un límite de cantidad, no una garantía de relevancia. Lab 10b abordará ese problema; en 10a no filtramos por score mínimo.

[Volver al laboratorio](../README.md).
