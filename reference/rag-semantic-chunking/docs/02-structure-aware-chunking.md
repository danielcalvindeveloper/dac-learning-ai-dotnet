# Structure-aware y tipo documental

Muchas veces **el autor ya expresó parte de la estructura semántica** con títulos, secciones y listas. [StructureAwareChunkingStrategy](../src/RagSemanticChunking/StructureAwareChunkingStrategy.cs) utiliza esa señal explícita.

## Implementación

[DocumentText](../src/RagSemanticChunking/DocumentText.cs) parsea Markdown con **Markdig 1.3.2**, componente CommonMark maduro con AST y posiciones de origen. No construimos un parser Markdown con expresiones regulares. La aplicación usa `Markdown.Parse`, `HeadingBlock` y `Span.Start/End`; el texto de salida siempre se extrae de la cadena original normalizada.

Cada heading inicia una sección/subsección. Si esa unidad cabe en **800 UTF-16**, se conserva entera. Si excede el máximo, se agrupan bloques completos de Markdig sin excederlo. Un párrafo o lista demasiado grande se divide con el fallback recursivo del baseline, **sin overlap**. Un título introductorio breve puede constituir un chunk: esta estrategia prioriza estructura y máximo, no aplica el mínimo semántico.

No se fragmenta una lista que cabe dentro del máximo. `SectionAt` mantiene la jerarquía de headings y la almacena como ruta de sección del **inicio** del chunk. No equivale a etiquetar todas las ideas contenidas en el rango ni agrega títulos artificiales al texto.

Sin headings todavía se aprovechan los bloques. Un `.txt` puede leerse como párrafos y recurrir a cortes por tamaño; no se promete reconstruir estructura que el documento no expresa.

## Ventajas y límites

Determinista y sin IA para segmentar. Preserva contexto escrito por el autor y evita el costo de embeddings extra. Depende de la calidad de la estructura: un heading no garantiza cohesión, una sección puede mezclar temas y un título solo puede ser poco útil para retrieval.

Los tests verifican jerarquía, listas, secciones grandes, fallback y cobertura. [Código de pruebas](../tests/RagSemanticChunking.Tests/ChunkingTests.cs).

## Document-aware: otras señales

El tipo documental puede ser tan importante como el algoritmo. No todos los formatos deberían reducirse a texto plano con el mismo criterio:

| Tipo | Señal útil para segmentar |
|---|---|
| Markdown y documentación técnica | Headings, listas, ejemplos y bloques de código |
| HTML | Árbol DOM y eliminación deliberada de navegación |
| PDF | Orden de lectura, páginas, columnas y encabezados |
| Tablas | Filas junto con encabezados y unidades de medida |
| Código fuente | Funciones, tipos, comentarios e imports relacionados |
| Documentos jurídicos | Artículos, incisos, referencias y definiciones |
| Formularios | Relación etiqueta/campo y grupos de respuestas |

Aquí sólo se implementa Markdown/texto. PDF/OCR/HTML, parsers de código y tablas complejas requieren componentes y decisiones propias. No se simulan dentro de este reference.

Fuentes: [Markdig oficial](https://github.com/xoofx/markdig), [paquete fijado estable](https://www.nuget.org/packages/Markdig/1.3.2), [CommonMark](https://spec.commonmark.org/0.31.2/).

[README](../README.md) · [Embedding semantic](03-embedding-semantic-chunking.md).
