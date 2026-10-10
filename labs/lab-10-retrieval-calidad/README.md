# Lab 10 - Retrieval y calidad

## Objetivo

Cerrar el bloque introductorio/intermedio de RAG trabajando múltiples documentos, relevancia y fuentes. Después de observar el pipeline en Lab 09a y encapsularlo en Lab 09b, podremos experimentar con la calidad del contexto que recuperamos.

## Estado

✅ Implementado mediante [Lab 10a - Múltiples documentos](lab-10a-multiples-documentos/README.md) y [Lab 10b - Relevancia, threshold y fuentes](lab-10b-relevancia-threshold-fuentes/README.md). Ambos sublabs son ejecutables e independientes.

## Estructura

Los dos sublabs disponibles respetan la convención del repositorio:

```text
lab-10-retrieval-calidad/
├── README.md
├── lab-10a-multiples-documentos/
│   ├── documents/
│   ├── docs/
│   └── Lab10a.MultiplesDocumentos.csproj
└── lab-10b-relevancia-threshold-fuentes/
    ├── documents/
    ├── docs/
    └── Lab10b.RelevanciaThresholdFuentes.csproj
```

Cada sublab incluye el mismo corpus y sus propias piezas conocidas, sin dependencia de ejecución hacia el otro.

## Lab 10a - Múltiples documentos

**Estado:** ✅ Implementado.

Pasamos de un documento a un corpus de conocimiento:

```text
documento A ─┐
documento B ─┼→ chunks → embeddings → índice en memoria
documento C ─┘
```

Cada chunk conserva `Source` y su posición local. Cuatro documentos Markdown alimentan un único índice en memoria; la consulta compara contra todos sus fragmentos. Se conservan las llamadas a los modelos configurados y no se agrega infraestructura de almacenamiento externa.

El pipeline permanece explícito en `Program.cs`. [README, configuración y ejecución de 10a](lab-10a-multiples-documentos/README.md).

## Lab 10b - Relevancia, threshold y fuentes

**Estado:** ✅ Implementado.

`topK = 3` selecciona hasta los tres mejores resultados disponibles. Si todos tienen poca relación con la pregunta, siguen siendo los mejores del índice. Cantidad no equivale a relevancia.

`Program.cs` muestra primero los candidatos de `Search` y después filtra por `minimumSimilarity = 0.70`, un valor experimental comprobado con este corpus y las preguntas del ejercicio:

```text
pregunta → retrieval → ¿hay resultados suficientemente relevantes?
                         ├── no → informar que no hay contexto suficiente
                         └── sí → construir contexto → LLM → respuesta + fuentes
```

La consulta relacionada acepta contexto y continúa hacia generación. La consulta de fotosíntesis también obtiene candidatos, pero en la prueba quedan bajo el mínimo y no se llama al LLM. Las fuentes mostradas provienen directamente de los resultados aceptados que se enviaron en el contexto.

El threshold necesita calibración y evaluación con preguntas concretas. No existe un valor universal para todos los modelos, corpus y consultas, y similarity no es un porcentaje de relevancia. Mostrar fuentes tampoco garantiza por sí solo que la respuesta sea correcta.

[README, experimento y ejecución de 10b](lab-10b-relevancia-threshold-fuentes/README.md).

## Cierre del bloque RAG

Con Lab 10 completamos el recorrido de embeddings, document loading, chunking, RAG explícito, `RagService`, múltiples documentos, retrieval, relevancia y fuentes. Es una base práctica para comprender sistemas RAG.

Las bases vectoriales reales, filtros por metadata, hybrid search, BM25, re-ranking, query rewriting, multi-query, chunking semántico, evaluación avanzada, observabilidad y citaciones avanzadas quedan como posibles áreas de una [Extensión - RAG avanzado](../../ROADMAP.md#extensión---rag-avanzado), fuera del recorrido principal.

## Siguiente laboratorio

[Lab 11 - Tools](../lab-11-tools/README.md), implementado: 11a muestra el ciclo explícito y 11b automatiza su coordinación. Ambos conservan la representación nativa recibida del SDK OpenAI sin condiciones por proveedor. Gemini y `thought_signature` son el caso real que hizo visible esa necesidad.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
