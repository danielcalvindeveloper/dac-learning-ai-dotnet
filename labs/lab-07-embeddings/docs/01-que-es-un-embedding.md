# Lectura opcional - Qué es un embedding

## Representación vectorial

Un modelo de embeddings transforma texto en un vector: una secuencia de números que representa características aprendidas de su significado.

```text
texto → modelo de embeddings → vector
```

La dimensión es la cantidad de números del vector. Depende del modelo y de su configuración; el laboratorio muestra la dimensión recibida.

## Similitud semántica

Textos relacionados tienden a producir vectores cercanos. Una consulta sobre una notebook para programar puede relacionarse con un documento sobre una laptop para desarrollo, aunque usen palabras diferentes.

Los valores individuales no se interpretan manualmente: no podemos señalar un número y afirmar que significa “computadora”. La representación funciona como conjunto.

Compará embeddings del mismo modelo y configuración. Dos modelos pueden producir espacios distintos incluso con igual dimensión.

## Generar texto y generar embeddings

Un modelo de chat genera mensajes para responder instrucciones. Un modelo de embeddings devuelve una representación numérica para comparar o procesar textos.

Son capacidades distintas: no todos los modelos soportan embeddings y pertenecer al mismo proveedor no implica que un modelo pueda realizar ambas tareas. Por eso distinguimos `AI_MODEL` de `AI_EMBEDDING_MODEL`; la [lectura de configuración](04-configuracion-y-embedding-generator-factory.md) explica cuándo aparece esta separación.

Generar el embedding de una consulta no responde la consulta. En este laboratorio solo medimos su relación con dos textos.

La similitud puede perder matices y no demuestra equivalencia, veracidad ni relevancia absoluta.
