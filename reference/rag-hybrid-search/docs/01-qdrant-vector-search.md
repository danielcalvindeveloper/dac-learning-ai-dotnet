# Qdrant y vector search

Se mantiene Qdrant mediante Microsoft.Extensions.VectorData y embeddings explícitos de Microsoft.Extensions.AI. La colección dac_hybrid_documents conserva vectores y payload; dac_rag_documents sigue independiente.

DocumentRecord define Guid como key, vector ReadOnlyMemory<float> con Cosine/HNSW, texto, fuente, posición, departamento, tipo, año y Generation. Los tres campos de filtro tienen IsIndexed = true.

VectorSearchService recibe generador, VectorStore y nombre de colección. Comprueba existencia, genera el embedding de la pregunta y valida dimensión. SearchAsync solicita hasta seis candidatos con VectorSearchOptions.Filter. El provider ejecuta el filtro nativo en Qdrant antes del topK; no calculamos coseno manualmente.

El modo vector obtiene su ranking exclusivamente de Qdrant. Lee metadata del commit Lucene para comprobar modelo/dimensión/generación, pero no consulta el índice lexical ni carga el corpus completo.

Con Cosine, mayor score indica cercanía; no probabilidad de respuesta correcta. Se aceptan scores finitos >= 0.60, umbral experimental del ejemplo. No se aplica a BM25 o RRF y debe revisarse al cambiar modelo/corpus.

## Consistencia

Se comprueba nombre del modelo antes de consultar y dimensión antes de buscar. Los hits deben tener la Generation del commit Lucene abierto. Una publicación parcial puede causar error y exigir reingesta.

Este control no certifica igualdad completa entre índices ni detecta cambios internos del proveedor bajo el mismo nombre. Igual dimensión no significa espacios compatibles. Identidad y publicación se explican en [ingesta dual](05-ingesta-dual.md).

Fuentes oficiales: [VectorData](https://learn.microsoft.com/en-us/dotnet/ai/vector-stores/overview), [IEmbeddingGenerator](https://learn.microsoft.com/en-us/dotnet/ai/iembeddinggenerator), [provider Qdrant](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/qdrant-connector), [puntos/upsert](https://qdrant.tech/documentation/manage-data/points/). La URL histórica del provider no incorpora Semantic Kernel a la aplicación.

[Arquitectura](00-arquitectura.md) · [Lucene](02-lucene-bm25.md).

