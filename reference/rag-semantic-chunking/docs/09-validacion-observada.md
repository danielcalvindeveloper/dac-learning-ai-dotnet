# Validación observada

Ejecución local del 11 de octubre de 2026. .NET SDK **10.0.401**, Qdrant **1.19.2** por Docker, endpoint compatible configurado en el `.env` raíz. Embeddings **gemini-embedding-001**, dimensión real **3072**; chat/segmentación **gemini-3.5-flash-lite**. No se imprimieron credenciales ni se cambiaron modelos para favorecer estrategias.

## Corpus y parámetros

| Documento | UTF-16 normalizado |
|---|---:|
| incidentes.md | 2828 |
| incorporacion.md | 2724 |
| licencias.md | 2518 |
| seguridad.md | 2696 |
| Total | 10766 |

Corpus SHA256: `FDBC3C6B0BB4277351EB758859EBF114E6224F0C3C5B2D5E49AD2B75A7B656F6`. Queries v1, diez preguntas. TopK=3 chunks para todos; coseno, mismo vector de consulta, sin threshold/filtro/hybrid/reranking. Prompt/chat/temperatura cero comunes para RAG.

Máximo 800, overlap fixed 100, mínimo semantic/llm 120, unidades iniciales de hasta 400. Semantic acepta coseno adyacente **<0.72** respetando min/max. Structure usa secciones/headings, bloques y fallback recursivo. LLM elige un `EndUnit` de un enum factible para el inicio actual, con extracción original y validador completo; no tiene fallback.

## Resultados medidos

| Estrategia | Chunks | Media | Min/max | Boundaries | Embeddings extra | LLM calls | Chunking aproximado | Recall@3 | MRR |
|---|---:|---:|---|---:|---:|---:|---:|---:|---:|
| fixed | 20 | 618,3 | 331/771 | 16 | 0 | 0 | 5 ms | 0,95 | 1,00 |
| structure | 20 | 538,3 | 256/646 | 16 | 0 | 0 | 9 ms | 1,00 | 1,00 |
| semantic | 30 | 358,9 | 208/646 | 26 | 60 | 0 | 4,45 s | 0,95 | 1,00 |
| llm | 24 | 448,6 | 231/663 | 20 | 0 | 21 | 76,25 s | 0,95 | 1,00 |

Los embeddings finales comunes fueron 20/20/30/24, además de los 60 por unidades de semantic. Las llamadas LLM son decisiones lógicas completadas; no cuentan reintentos HTTP del transporte ni intentos de desarrollo fallidos. El tiempo LLM incluye **una espera de 60 segundos ante HTTP 429**. Son tiempos observados de chunking, no de ingesta completa ni benchmarks de rendimiento.

Q01–Q09 tuvieron Recall=1 en todas las variantes. En Q10, que espera licencias e incidentes, structure recuperó ambos documentos; las otras tres sólo licencias, Recall=0,5. El promedio resultó 1,00 frente a 0,95. MRR=1 indica que el primer hit siempre perteneció a algún documento esperado; no prueba que se recuperara toda la evidencia necesaria.

Los reportes detallados reproducibles quedan en `artifacts/comparison.json` y `artifacts/comparison.md`, ignorados por Git. Los cuatro manifiestos conservan límites, motivos, texto, settings, similitudes adyacentes y métricas por documento. La tabla es una observación, no datos codificados en el programa.

## Ejecuciones reales

Las cuatro ingestas completaron y publicaron `dac_chunking_fixed` (20), `dac_chunking_structure` (20), `dac_chunking_semantic` (30) y `dac_chunking_llm` (24), con dimensión 3072.

Se ejecutó `compare` sobre las diez preguntas y `query` con la misma pregunta de pérdida del teléfono MFA para las cuatro estrategias. Las cuatro completaron retrieval + generación y devolvieron SEG-8, verificación por llamada al contacto registrado y objetivo de cuatro horas hábiles, con fuentes calculadas desde contexto. Eso comprueba el flujo; no garantiza respuestas idénticas o correctas en cualquier ejecución.

La estrategia LLM pasó validación de posiciones, continuidad, cobertura y tamaños; sus chunks se construyeron exclusivamente con slices originales. Antes de finalizar se rechazaron propuestas inválidas y se observó HTTP 429 sin publicación parcial durante preparación. Se mejoró el contrato para restringir un límite por decisión y se extendió la política SDK ante cuota temporal, sin introducir reparación de contenido o fallback oculto.

## Tests y documentación

`dotnet restore` completó correctamente. `dotnet build` completó con **cero warnings y cero errores**. `dotnet test` pasó **55 tests offline**, y el proyecto separado pasó un test de integración Qdrant. La integración ejecuta ingesta/reingesta de las cuatro estrategias, IDs, payload, búsqueda, reporte y rechazo de generación incompatible, con colecciones temporales e IA controlada.

Se auditó también el contenido original, cobertura, min/max, generación y metadata de **todos los puntos** de las cuatro colecciones mediante REST. Se renderizaron los cinco Mermaid a SVG. La revisión de **635 enlaces locales en 146 archivos Markdown** no encontró destinos o anchors rotos. Las colecciones previas `dac_rag_documents` y `dac_hybrid_documents` permanecen presentes.

Se crearon **48 archivos** bajo esta referencia: 20 archivos C# de aplicación, tres csproj (consola y dos proyectos de tests), cinco archivos C# de pruebas, solución/global.json, Compose/ejemplo de entorno, cuatro documentos, queries.json, README y diez guías. El [mapa de artefactos](00-arquitectura.md) y la [guía de tests](07-testing.md) enlazan el código. Manifiestos, reportes y renders están generados en `artifacts/`, ignorada por Git.

Se modificaron únicamente ocho documentos existentes: README raíz, ROADMAP, `reference/README.md`, README de vector-store e hybrid-search y las guías generales `docs/00-vision-y-alcance.md`, `docs/03-roadmap-labs.md` y `docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md`. El bloque numerado del roadmap permaneció idéntico y no se modificaron archivos de Labs. Semantic Chunking queda implementada; Advanced Retrieval, Evaluation y Agent Framework continúan planificadas.

Se compilaron APIs reales de MEAI/VectorData/Markdig con .NET 10. La política extiende `System.ClientModel.Primitives.ClientRetryPolicy.GetNextDelay`; Structured Output usa `ChatResponseFormat.ForJsonSchema(JsonElement, ...)`, sin agregar el paquete completo MEAI. Se usa Markdig estable 1.3.2 para AST/posiciones, no un parser artesanal. [Mapa de dependencias](../README.md#tecnologías-y-motivos).

## Decisiones y límites de la evidencia

La unidad inicial es bloque Markdown acotado, no cada oración: reduce embeddings y conserva una señal observable. El costo adicional semántico se registra, en lugar de ocultarse. La variante LLM usa múltiples decisiones acotadas para preservar constraints y recibe contexto repetido; esa confiabilidad de formato tiene costo/latencia.

No se agregó MAF, MCP, Lucene, BM25, RRF, agentes ni workflows. No se evaluaron automáticamente respuestas ni se ejecutaron otros proveedores. No se afirma un ganador universal ni se crean referencias futuras. El resto del recorrido de Labs conserva su estado.

[README](../README.md) · [Comparación](05-comparacion-y-evaluacion.md) · [Testing](07-testing.md).
