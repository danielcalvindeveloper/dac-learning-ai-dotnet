# Implementaciones de referencia

## Propósito

Las implementaciones de referencia muestran cómo aplicar los conceptos de los laboratorios utilizando librerías, frameworks e infraestructura más cercanos a una aplicación real.

Buscan reducir la distancia entre “entiendo cómo funciona” y “veo cómo podría implementarlo en un proyecto real”. Forman parte del mismo proyecto y complementan el recorrido incremental de `labs/`.

**Estado actual:** [RAG con Vector Store real](rag-vector-store/README.md) y [RAG Hybrid Search + BM25](rag-hybrid-search/README.md) están implementadas y validadas con Qdrant. RAG Semantic Chunking y el agente con Microsoft Agent Framework continúan planificados.

## Cómo recorrer ambos niveles

```text
labs/                  reference/
comprender el mecanismo → ver una implementación aplicada
                              ↓
                     relacionar ambos niveles
```

Primero entender, después abstraer y finalmente integrar. Una implementación pedagógica explícita tiene un objetivo propio: permitir observar el mecanismo. Una implementación aplicada presupone esa comprensión y muestra qué responsabilidad conviene delegar.

Podés volver al [recorrido de laboratorios](../docs/03-roadmap-labs.md) o explorar la [carpeta labs/](../labs/). Las referencias podrán desarrollarse en paralelo cuando el bloque conceptual correspondiente tenga suficiente madurez, sin esperar a finalizar todos los labs.

## Realista ≠ industrial

Las futuras referencias deben ser ejecutables, comprensibles, suficientemente pequeñas para estudiar y razonablemente cercanas a un escenario real.

No buscan ser plantillas universales ni arquitecturas enterprise completas. Evitamos por defecto microservicios innecesarios, Kubernetes, seguridad enterprise, observabilidad completa, CI/CD complejo y capas que no aporten valor pedagógico.

La infraestructura debe resolver una necesidad visible del caso. Incorporarla no convierte a los labs en implementaciones incorrectas ni elimina su valor.

## Selección de tecnologías

**Primero el problema, después la herramienta.**

```text
problema → concepto → abstracción → herramienta adecuada
```

Cada referencia debe justificar qué aporta la tecnología elegida, qué complejidad agrega y qué conserva de los labs. No creamos un proyecto sólo porque una librería figure en el stack.

Las Implementaciones de Referencia deben privilegiar componentes y librerías maduras cuando existan: muestran cómo integrar correctamente infraestructura en una aplicación realista. El tamaño pequeño del dataset facilita ejecutar, leer y comparar; **no es un argumento para sustituir motores reales por implementaciones artesanales**.

La lógica propia corresponde cuando pertenece a la solución y mejora claridad. RRF propio compone rankings y es apropiado; BM25 artesanal en lugar de Lucene.NET no corresponde a esta referencia. Esta elección admite documentar compromisos reales: Lucene.NET 4.8 utiliza paquetes prerelease, verificados y fijados, sin presentarlos como estables.

Los nombres de carpeta deben ser breves, descriptivos, en minúsculas y con guiones. Este carril no usa números de Lab ni modifica milestones. No se crean carpetas vacías para candidatas futuras.

## Estrategia de librerías y frameworks

### Microsoft.Extensions.AI

Es la base común de abstracciones del recorrido, como `IChatClient` e `IEmbeddingGenerator`. Permite conservar conceptos compartidos al cambiar el cliente concreto o incorporar componentes de mayor nivel. [Documentación oficial](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai).

### Microsoft.Extensions.VectorData y vector stores

Es la abstracción de infraestructura especializada para almacenar registros y buscar vectores en RAG. Complementa los embeddings de Microsoft.Extensions.AI; la persistencia y la búsqueda corresponden al store concreto. `rag-vector-store/` ya utiliza Qdrant mediante este contrato. [Documentación oficial](https://learn.microsoft.com/en-us/dotnet/ai/vector-stores/overview).

### Microsoft Agent Framework

Es el framework objetivo del proyecto para nuevos escenarios agénticos: agentes, sesiones, workflows y coordinación multi-agent, **después de comprender el Agent Loop**. También ofrece capacidades de harness para tareas de varios pasos; se incorporarán sólo si el problema las necesita. La candidata `agent-framework/` retomará la familia de problemas de soporte de Lab 13. [Descripción oficial](https://learn.microsoft.com/en-us/agent-framework/overview/).

### Relación entre las capas

Microsoft.Extensions.AI es la base para chat, embeddings, Tools y salida estructurada. Microsoft.Extensions.VectorData aporta los contratos vectoriales. Microsoft Agent Framework utiliza tipos de Microsoft.Extensions.AI y agrega infraestructura para escenarios agénticos; no reemplaza esa base ni exige introducir una capa intermedia de frameworks.

No todo problema requiere un agente. La referencia RAG resuelve recuperación y generación con sus abstracciones e infraestructura vectorial. La referencia agéntica estudiará qué delegar al framework una vez comprendidos objetivo, estado, decisión, acción, observación, iteración y finalización.

Semantic Kernel forma parte de la evolución previa del ecosistema Microsoft AI. Puede aparecer en documentación histórica de conectores o en migraciones de sistemas existentes, pero no es una tecnología objetivo de nuevas implementaciones de este proyecto. La [guía oficial de migración](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel/) aporta ese contexto y documenta el uso de tipos comunes de Microsoft.Extensions.AI en Agent Framework.

## Primeras implementaciones y candidatas

El carril RAG evoluciona de Vector Store real a Hybrid Search y después a Semantic Chunking. Las dos primeras referencias son ejecutables; Semantic Chunking y el agente con Microsoft Agent Framework siguen siendo propuestas, sin carpetas creadas:

| Ruta | Pregunta que resuelve o resolverá | Estado |
|---|---|---|
| [`reference/rag-vector-store/`](rag-vector-store/README.md) | ¿Cómo reemplazar `InMemoryVectorStore` y la búsqueda lineal por infraestructura vectorial real? | ✅ Implementada |
| [`reference/rag-hybrid-search/`](rag-hybrid-search/README.md) | ¿Cuándo combinar similitud semántica con BM25, RRF y filtros por metadata? | ✅ Implementada |
| `reference/rag-semantic-chunking/` | ¿Qué ocurre si el problema está en cómo construimos los chunks? | ⏳ Planificada |
| `reference/agent-framework/` | ¿Qué infraestructura de un agente de soporte puede delegarse a Microsoft Agent Framework después de comprender el loop explícito? | ⏳ Planificada |

### RAG con Vector Store real

Parte de [Lab 07 — Embeddings](../labs/lab-07-embeddings/README.md), [Lab 08 — Documentos y chunking](../labs/lab-08-document-loading/README.md), [Lab 09 — RAG básico](../labs/lab-09-rag-basico/README.md) y [Lab 10 — Retrieval y calidad](../labs/lab-10-retrieval-calidad/README.md).

| Conceptos visibles en los labs | Aplicación en la referencia |
|---|---|
| Chunks y embeddings | Chunks y embeddings conservados |
| `VectorSimilarity` y `InMemoryVectorStore` | Vector store real, persistencia, indexación y vector search |
| `topK` y threshold experimental | Recuperación y criterios de aceptación documentados |
| Origen de los fragmentos y RAG | Metadata y RAG con infraestructura especializada |

La [implementación con Qdrant](rag-vector-store/README.md) conserva embeddings explícitos mediante Microsoft.Extensions.AI y delega persistencia y vector search a Microsoft.Extensions.VectorData. Se validaron ingesta, consultas, fuentes, persistencia, IDs estables y tests unitarios.

### RAG Hybrid Search + BM25

Continúa [rag-vector-store](rag-vector-store/README.md) y el bloque conceptual de Labs 07–10. La [implementación híbrida](rag-hybrid-search/README.md) conserva Qdrant y embeddings explícitos, delega BM25 e índice invertido persistente a Lucene.NET y fusiona posiciones mediante RRF propio. Ambos motores aplican filtros nativos antes del topK. Lucene.Net y Analysis.Common están fijados en 4.8.0-beta00018, **prerelease**, con .NET 10 verificado.

Incluye comparación reproducible de consultas semánticas, códigos exactos y evidencia complementaria, RAG completo con fuentes, unit tests offline y un proyecto separado de integración. Su [documentación técnica](rag-hybrid-search/README.md#documentación-técnica) explica cada componente. Hybrid no garantiza mejorar todas las preguntas.

La siguiente candidata del carril RAG es `reference/rag-semantic-chunking/`, todavía sin implementar: estudiar cómo la construcción de chunks afecta la recuperación.

### Agente con Microsoft Agent Framework

Parte de [Lab 11 — Tools](../labs/lab-11-tools/README.md), [Lab 12 — Function Calling](../labs/lab-12-function-calling/README.md) y especialmente [Lab 13 — Primer agente](../labs/lab-13-primer-agente/README.md).

```text
Lab 13
objetivo → estado → decisión → acción → observación → iteración
    ↓
reference/agent-framework/
misma familia de problemas de incidentes
infraestructura agéntica delegada al framework
```

La referencia deberá permitir relacionar el loop conceptual con las responsabilidades asumidas por Microsoft Agent Framework. Lab 13 conserva su implementación explícita, sin migrarlo al framework. Primero entender el loop, después usar el framework. La referencia no anticipa la implementación de Labs 14 o 15 ni presupone incorporar workflows o múltiples agentes al caso.

## Navegación Labs ↔ Reference

Cada implementación tiene una sección **Conocimientos previos** con enlaces a los README de los labs que aplica. [RAG con Vector Store real](rag-vector-store/README.md#conocimientos-previos) ya permite recorrer ambos niveles desde Labs 07–10.

Los README de Labs 07–10 enlazan la referencia ejecutable. Mientras una candidata siga planificada, no agregamos enlaces a proyectos inexistentes ni placeholders en todos los labs.

## Documentación obligatoria de cada implementación

**Norma permanente, sin excepción, para todas las referencias actuales y futuras:** cada `reference/*` debe incluir `README.md`, `docs/`, código y tests. Código maduro no significa sobrearquitectura.

`docs/` debe explicar arquitectura, artefactos relevantes, dependencias y su motivo, decisiones de diseño, entorno, ejecución y testing; incluir diagramas Mermaid y referencias oficiales donde corresponda. El README debe enlazar todos esos documentos. No alcanza con README y código. Los comentarios pedagógicos del código deben explicar decisiones y conceptos no obvios para que un desarrollador menos experimentado pueda seguir la solución.

Cada implementación debe tener su propio `README.md` con, al menos:

1. **Objetivo:** qué se pretende observar o construir.
2. **Problema que resuelve:** caso concreto y alcance.
3. **Conocimientos previos:** labs conceptuales relacionados, con enlaces.
4. **Arquitectura general:** piezas principales y sus responsabilidades.
5. **Librerías/frameworks utilizados:** dependencias significativas.
6. **Por qué se eligieron:** aporte y alternativas relevantes para el caso.
7. **Requisitos previos:** conocimientos, herramientas y versiones necesarias.
8. **Preparación del entorno:** instalación y puesta en marcha.
9. **Configuración:** qué requiere la aplicación.
10. **Variables de entorno:** significado, obligatoriedad y valores de ejemplo.
11. **Infraestructura necesaria:** servicios y cómo verificarlos.
12. **Cómo ejecutar:** comandos reproducibles y resultado esperado.
13. **Recorrido del código:** dónde seguir el flujo principal.
14. **Diferencias respecto del lab conceptual:** qué se delegó y qué permanece.
15. **Limitaciones:** alcance y compromisos de la implementación.
16. **Posibles siguientes pasos:** mejoras justificadas por el problema.

### Dependencias significativas

Por cada librería o framework relevante se explica brevemente **qué es, para qué se usa, qué responsabilidad resuelve y por qué fue elegido**. Se agregan enlaces oficiales cuando aportan valor, sin copiar documentación extensa.

No es necesario reproducir esta explicación para cada dependencia transitiva.

### Preparación de infraestructura

Si el caso requiere Docker, una base vectorial, un servicio local, una API o alguna CLI, su README debe indicar:

- qué instalar y si es obligatorio u opcional;
- la versión esperada cuando importe;
- cómo iniciar y preparar el servicio;
- cómo verificar que funciona;
- cómo detenerlo cuando corresponda.

Si no requiere infraestructura adicional, también debe indicarlo. La preparación debe permitir ejecutar el ejemplo sin asumir servicios previamente configurados.

### Configuración común

Se respetará, cuando resulte razonable, la configuración del repositorio:

```env
AI_PROVIDER=
AI_MODEL=
AI_EMBEDDING_MODEL=
AI_API_KEY=
AI_URL=
```

Cada implementación requerirá sólo las capacidades que utilice. Podrá incorporar variables adicionales si su infraestructura lo necesita y deberá documentarlas; no definimos variables nuevas para candidatas todavía inexistentes.

## Continuar el recorrido

[README principal](../README.md) · [Labs](../labs/) · [Recorrido conceptual](../docs/03-roadmap-labs.md) · [Roadmap y candidatas](../ROADMAP.md#implementaciones-de-referencia) · [Enfoque pedagógico](../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
