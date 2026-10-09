# Lectura opcional - TopK y relevancia

El ranking compara candidatos dentro del corpus. Siempre puede haber un mejor resultado disponible aunque ninguno responda la pregunta. `topK` limita cuántos candidatos devuelve la búsqueda, no cuánta evidencia útil contienen.

## Dos etapas visibles

```text
Search → ordenar por similitud → hasta topK candidatos
                                      ↓
                    filtrar Similarity >= minimumSimilarity
                                      ↓
                              resultados aceptados
```

El filtrado no vuelve a ordenar ni rellena la lista con otros candidatos. Pueden quedar menos de K resultados, incluso cero.

En el experimento, tanto la consulta sobre RAG como la de fotosíntesis recuperan candidatos. Observar ambos rankings permite reconocer el problema antes de aplicar la regla de aceptación.

`relevantResults` significa que los resultados alcanzaron el mínimo elegido. El nombre no implica que hayamos demostrado relevancia absoluta. El texto aceptado todavía debe revisarse en relación con la pregunta.

[Volver al laboratorio](../README.md).
