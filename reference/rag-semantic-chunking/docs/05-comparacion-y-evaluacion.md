# Comparación y evaluación

## Qué se mantiene constante

Mismos cuatro documentos, mismas diez preguntas versionadas, proveedor/endpoint, modelo de embeddings, dimensión, Qdrant y búsqueda dense con coseno. **TopK=3 chunks**, sin threshold de aceptación, filtros, reranking ni hybrid. El contexto y prompt de `RagService`, el chat configurado y temperatura cero son iguales para consultas de cualquier estrategia.

Se usa un único vector por pregunta para consultar las cuatro colecciones durante `compare`. No se vuelven a generar embeddings idénticos por estrategia. El comando no llama a chat: mide retrieval y costos de la segmentación. Para comparar respuestas manualmente, ejecutá la misma pregunta con `query --chunking ...` en cada variante.

## Corpus y verdad esperada

El corpus ficticio Aurora contiene [incorporación](../data/incorporacion.md), [seguridad](../data/seguridad.md), [incidentes](../data/incidentes.md) y [licencias](../data/licencias.md). Tiene headings, subsecciones, párrafos, listas, cambios de tema y relaciones cruzadas. Sus políticas no son reglas legales ni procedimientos reales.

[queries.json](../examples/queries.json) versión 1 define diez consultas: ocho centradas en un documento y dos que necesitan información de dos documentos. La relevancia se establece por nombre de documento, nunca por ChunkId: distintos boundaries siguen evaluándose contra el mismo material esperado. No se cambian preguntas o corpus según estrategia.

Es una evaluación pequeña y conocida por el desarrollador, no un benchmark independiente. Documento correcto no garantiza fragmento suficiente. Una evaluación futura podría anotar secciones/evidencias y evaluar respuestas, sin mezclar ese alcance aquí.

## Recall@3 y MRR

```text
Recall@3 = documentos relevantes distintos presentes entre los 3 chunks
           / documentos relevantes esperados

RR = 1 / posición del primer chunk de un documento relevante
MRR = promedio de RR sobre las preguntas
```

Si la pregunta espera dos documentos y sólo uno aparece, Recall=0.5. Dos chunks del mismo documento no cuentan como dos documentos recuperados. Sin coincidencias Recall y RR son cero. El topK continúa siendo de chunks: **no se colapsa el ranking ni se recuperan tres documentos distintos**. MRR sólo observa la primera evidencia relevante, no penaliza que falte la segunda; por eso acompaña a Recall.

[Evaluation](../src/RagSemanticChunking/Evaluation.cs) implementa estas funciones pequeñas y deterministas. `QuerySet` valida versión, IDs, preguntas y documentos existentes. `Summarize` exige una medición por consulta/estrategia. `ComparisonService` coordina retrieval real y escribe `artifacts/comparison.json` y `artifacts/comparison.md`.

## Comparabilidad y métricas operativas

Antes de consultar, se requieren cuatro manifiestos con el mismo hash del corpus actual, settings y dimensión. También se comprueban proveedor/endpoint y ambos modelos contra la configuración actual. El hash no incluye credenciales. Los hits deben coincidir con la generación de su manifiesto.

El reporte incluye por estrategia: cantidad de chunks, tamaño medio/mínimo/máximo, boundaries internos por documento, embeddings adicionales, invocaciones LLM, tiempo acumulado de chunking, Recall medio y MRR. Cada pregunta conserva hits, fuentes, sección y score en JSON. Los manifiestos contienen texto y límites para reconstruir el porqué.

`ChunkingMilliseconds` incluye parsing/unidades y llamadas requeridas por la estrategia dentro de `ChunkAsync`; el parseo inicial común del documento ocurre antes y no se incluye. No incluye embeddings de los chunks finales, Qdrant ni generación RAG. No es latencia end-to-end ni una factura. Embeddings adicionales cuenta **entradas**, no requests. El número de embeddings comunes de indexación es igual al número de chunks y también cambia al cambiar la segmentación.

## Interpretación honesta

No se hardcodea un ganador. Con pocos documentos varias estrategias pueden empatar en Recall y diferir en MRR, límites, costo o suficiencia de contexto. Un resultado más caro no prueba una mejora. No se afirma groundedness ni exactitud a partir de coincidencia de fuentes.

Para repetir: conservar corpus, queries, modelos y settings; ejecutar las cuatro ingestas, luego `compare`. Conservar manifiestos si se quiere estudiar exactamente una segmentación LLM, sin volver a generarla. Cambiar modelos requiere reiniciar las colecciones dedicadas según [entorno](08-entorno-y-ejecucion.md).

Fuente primaria para métricas de recuperación: [Introduction to Information Retrieval, evaluación](https://nlp.stanford.edu/IR-book/html/htmledition/evaluation-of-ranked-retrieval-results-1.html).

[README](../README.md) · [Testing](07-testing.md).
