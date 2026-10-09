# Lab 10 - Retrieval y calidad

## Objetivo

Cerrar el bloque introductorio/intermedio de RAG trabajando múltiples documentos, relevancia y fuentes. Después de observar el pipeline en Lab 09a y encapsularlo en Lab 09b, podremos experimentar con la calidad del contexto que recuperamos.

## Estado

🚧 En progreso. [Lab 10a - Múltiples documentos](lab-10a-multiples-documentos/README.md) está implementado. Lab 10b - Relevancia, threshold y fuentes continúa pendiente; Lab 10 no está completo.

## Estructura

El sublab disponible respeta la convención del repositorio:

```text
lab-10-retrieval-calidad/
├── README.md
└── lab-10a-multiples-documentos/
    ├── documents/
    ├── docs/
    ├── Lab10a.MultiplesDocumentos.csproj
    └── ...
```

`lab-10b-relevancia-threshold-fuentes/` es la ruta prevista para el siguiente sublab. Todavía no se crea su carpeta ni implementación.

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

**Estado:** ⏳ Pendiente.

`topK = 3` selecciona hasta los tres mejores resultados disponibles. Si todos tienen poca relación con la pregunta, siguen siendo los mejores del índice. Cantidad no equivale a relevancia.

Introduciremos `minimumSimilarity`, o un concepto equivalente, para decidir qué fragmentos aceptamos como contexto:

```text
pregunta → retrieval → ¿hay resultados suficientemente relevantes?
                         ├── no → informar que no hay contexto suficiente
                         └── sí → construir contexto → LLM → respuesta + fuentes
```

El threshold necesita calibración y evaluación con preguntas concretas. No existe un valor universal para todos los modelos, corpus y consultas. Las fuentes recuperadas acompañarán la respuesta para observar de dónde salió el contexto; mostrarlas no garantiza por sí solo que la respuesta sea correcta.

## Cierre del bloque RAG

Al terminar Lab 10 habremos recorrido embeddings, document loading, chunking, RAG explícito, `RagService`, múltiples documentos, retrieval, relevancia y fuentes. Esa será una base práctica para comprender sistemas RAG.

Las bases vectoriales reales, filtros por metadata, hybrid search, BM25, re-ranking, query rewriting, multi-query, chunking semántico, evaluación avanzada, observabilidad y citaciones avanzadas quedan como posibles áreas de una [Extensión - RAG avanzado](../../ROADMAP.md#extensión---rag-avanzado), fuera del recorrido principal.

## Siguiente laboratorio

[Lab 11 - Primer Tool](../lab-11-tools/README.md), pendiente: una solicitud del modelo, argumentos, ejecución de código .NET y devolución del resultado.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
