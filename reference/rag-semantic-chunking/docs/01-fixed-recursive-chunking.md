# Fixed / Recursive: un baseline razonable

[FixedChunkingStrategy](../src/RagSemanticChunking/FixedChunkingStrategy.cs) recibe un documento y utiliza [TextPartition.Recursive](../src/RagSemanticChunking/TextPartition.cs). No llama al LLM ni genera embeddings para decidir límites. Los embeddings posteriores de indexación sí son comunes a todas las estrategias.

## Reglas visibles

1. Considerar como máximo **800 unidades UTF-16** desde el inicio actual.
2. Si falta texto, buscar hacia atrás un separador de párrafos dentro de la segunda mitad de la ventana.
3. Si no existe, buscar terminación `.`, `!` o `?` seguida de whitespace.
4. Si tampoco existe, cortar por tamaño, conservando parejas sustitutas Unicode.
5. La siguiente ventana comienza **100 unidades antes** del corte. Si ese inicio separaría una pareja sustituta, retrocede una unidad adicional.

Buscar sólo en la segunda mitad evita retroceder hasta un párrafo mínimo y conserva el avance. La última ventana termina en el final real, sin generar otra cola ya cubierta. No se aplica el mínimo semántico al último chunk fixed: puede ser corto. Los límites y motivos se guardan en el manifiesto.

Para ventanas diminutas, conservar Unicode y overlap simultáneamente puede impedir avanzar. El helper reduce overlap en ese caso excepcional; una ventana que no admite ni un carácter Unicode completo se rechaza con error. Los parámetros de este experimento no requieren esa reducción.

```text
chunk 0: [---------------------]
chunk 1:                  [---------------------]
                           overlap
```

Esta es una heurística recursiva por prioridades, no un tokenizer ni un segmentador lingüístico. La unidad de tamaño es UTF-16; no equivale a tokens, palabras o grafemas. El fallback puede partir palabras y secuencias visuales compuestas, aunque evita separar los dos elementos de una pareja sustituta.

## Ventajas y límites

**Fixed chunking no es una mala práctica.** Es barato, reproducible y sirve cuando las unidades tienen tamaño bastante regular. Párrafos/oraciones aportan límites naturales sin infraestructura adicional. El overlap reduce pérdidas de contexto en cortes arbitrarios y aumenta redundancia/cantidad de vectores.

No reconoce por sí solo cambios de tema, títulos o relaciones distribuidas. La detección de oración es deliberadamente simple: una abreviatura puede confundirse con un final. Ni el tamaño ni el overlap son valores universales. La comparación debe mostrar sus resultados reales, incluso si iguala o supera las variantes más costosas.

Tests: párrafo preferido, fallback por oración, corte forzado, máximo, Unicode, overlap, conservación y cancelación en [ChunkingTests](../tests/RagSemanticChunking.Tests/ChunkingTests.cs).

Fuente: [representación de strings y UTF-16 en .NET](https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-encoding-introduction).

[README](../README.md) · [Structure-aware](02-structure-aware-chunking.md).
