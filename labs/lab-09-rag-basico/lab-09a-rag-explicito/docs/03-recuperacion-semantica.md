# Lectura opcional - Recuperación semántica

```text
pregunta → embedding → comparar con chunks → ranking → topK
```

Utilizamos `AI_EMBEDDING_MODEL` tanto para los chunks como para la pregunta. `AI_MODEL` se utiliza después, durante la generación. Tener igual dimensión no basta para comparar vectores de modelos diferentes.

`Search(queryVector, topK)` recorre todos los elementos del store, calcula similitud coseno con `VectorSimilarity`, ordena de mayor a menor y toma como máximo `topK` resultados. Es búsqueda lineal, sin índice aproximado.

`SearchResult` contiene únicamente el chunk y la similitud calculada. El score no es una probabilidad y topK limita la cantidad, no garantiza relevancia. Sin score mínimo, el store puede devolver fragmentos poco relacionados.

`topK <= 0` lanza `ArgumentOutOfRangeException`. Un store vacío devuelve una colección vacía. Si hay menos de topK elementos, se devuelven los disponibles. Los vectores comparados deben ser no vacíos, finitos, de igual dimensión y con magnitud distinta de cero.

Estas validaciones evitan resultados matemáticamente inválidos. No evalúan si el contenido responde la pregunta; esa limitación se revisa en la [lectura de limitaciones](05-limitaciones-rag-basico.md).

[Volver al laboratorio](../README.md).
