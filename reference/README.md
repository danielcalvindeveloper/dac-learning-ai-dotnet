# Implementaciones de referencia

## Propósito

Las implementaciones de referencia muestran cómo aplicar los conceptos de los laboratorios utilizando librerías, frameworks e infraestructura más cercanos a una aplicación real.

Buscan reducir la distancia entre “entiendo cómo funciona” y “veo cómo podría implementarlo en un proyecto real”. Forman parte del mismo proyecto y complementan el recorrido incremental de `labs/`.

**Estado actual:** [RAG con Vector Store real](rag-vector-store/README.md) está implementada y validada con Qdrant. RAG con Semantic Kernel y el agente con Microsoft Agent Framework continúan planificados.

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

Los nombres de carpeta deben ser breves, descriptivos, en minúsculas y con guiones. Este carril no usa números de Lab ni modifica milestones. No se crean carpetas vacías para candidatas futuras.

## Estrategia de librerías y frameworks

### Microsoft.Extensions.AI

Es la base común de abstracciones del recorrido, como `IChatClient` e `IEmbeddingGenerator`. Permite conservar conceptos compartidos al cambiar el cliente concreto o incorporar componentes de mayor nivel. [Documentación oficial](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai).

### Semantic Kernel

Puede aportar integración de servicios AI, plugins/functions, conectores de vector stores y orquestación. Sigue siendo relevante para componentes concretos y también cuenta con capacidades agénticas. [Introducción oficial](https://learn.microsoft.com/en-us/semantic-kernel/overview/), [vector stores](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors) y [agentes de Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/).

**Debe aparecer tempranamente en `reference/` cuando aporte valor concreto**, especialmente alrededor de RAG e integración de infraestructura AI. La candidata prioritaria es `rag-semantic-kernel/`: observar su aporte a un problema ya comprendido, en lugar de crear un ejemplo genérico sólo para usar el framework.

### Microsoft Agent Framework

Es la referencia natural del proyecto para aplicar infraestructura especializada de agentes, sesiones, workflows y orquestación **después de comprender el Agent Loop**. La candidata `agent-framework/` retomará la familia de problemas de soporte de Lab 13. [Descripción oficial](https://learn.microsoft.com/en-us/agent-framework/overview/).

### Relación entre ambos frameworks

Semantic Kernel y Microsoft Agent Framework tienen áreas de superposición: ambos pueden trabajar con capacidades AI y Tools. Microsoft orienta los nuevos escenarios agénticos hacia Agent Framework; esto no convierte a Semantic Kernel en obsoleto ni implica un reemplazo universal de todos sus componentes. [Posición oficial sobre ambos frameworks](https://devblogs.microsoft.com/agent-framework/semantic-kernel-and-microsoft-agent-framework/).

Puede existir interoperabilidad mediante abstracciones y componentes compatibles del ecosistema .NET; por ejemplo, Agent Framework admite integraciones de vector stores basadas en `Microsoft.Extensions.VectorData`. La compatibilidad concreta se comprobará al implementar cada caso. [Integraciones oficiales de vector stores](https://learn.microsoft.com/en-us/agent-framework/integrations/by-component/vector-stores/).

No hay una jerarquía universal entre estas herramientas. Agent Framework no requiere incorporar Semantic Kernel como dependencia obligatoria del diseño: puede utilizar un cliente de chat y Tools mediante `Microsoft.Extensions.AI`. No es obligatorio usarlos juntos ni se agregan ambos por estar listados en el stack. La [guía oficial de transición](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel/) muestra las diferencias entre sus APIs.

## Primeras implementaciones y candidatas

La primera referencia ya es ejecutable. Las otras rutas siguen siendo propuestas, sin carpetas creadas:

| Ruta | Pregunta que resuelve o resolverá | Estado |
|---|---|---|
| [`reference/rag-vector-store/`](rag-vector-store/README.md) | ¿Cómo reemplazar `InMemoryVectorStore` y la búsqueda lineal por infraestructura vectorial real? | ✅ Implementada |
| `reference/rag-semantic-kernel/` | ¿Cómo construir una solución RAG de mayor nivel con Semantic Kernel después de comprender embeddings, chunking, retrieval y grounding? | ⏳ Planificada |
| `reference/agent-framework/` | ¿Qué infraestructura de un agente de soporte puede delegarse a Microsoft Agent Framework después de comprender el loop explícito? | ⏳ Planificada |

### RAG con Vector Store real

Parte de [Lab 07 — Embeddings](../labs/lab-07-embeddings/README.md), [Lab 08 — Documentos y chunking](../labs/lab-08-document-loading/README.md), [Lab 09 — RAG básico](../labs/lab-09-rag-basico/README.md) y [Lab 10 — Retrieval y calidad](../labs/lab-10-retrieval-calidad/README.md).

| Conceptos visibles en los labs | Aplicación en la referencia |
|---|---|
| Chunks y embeddings | Chunks y embeddings conservados |
| `VectorSimilarity` y `InMemoryVectorStore` | Vector store real, persistencia, indexación y vector search |
| `topK` y threshold experimental | Recuperación y criterios de aceptación documentados |
| Origen de los fragmentos y RAG | Metadata y RAG con infraestructura especializada |

La [implementación con Qdrant](rag-vector-store/README.md) conserva embeddings explícitos y delega persistencia y vector search a Microsoft.Extensions.VectorData. Se validaron ingesta, consultas, fuentes, persistencia, IDs estables y tests unitarios. No incorpora Semantic Kernel; la siguiente referencia estudiará su aporte adicional.

### RAG con Semantic Kernel

Aplica el mismo bloque conceptual de Labs 07–10 para mostrar qué integra y abstrae Semantic Kernel, qué código permanece en la aplicación y cómo se conserva el grounding: responder a partir del contexto recuperado.

Su pregunta es el aporte del framework a RAG; la candidata anterior estudia el cambio de infraestructura vectorial. Son problemas relacionados, pero diferentes.

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

La referencia deberá permitir relacionar el loop conceptual con las responsabilidades asumidas por el framework. No anticipa la implementación de Labs 14 o 15 ni presupone incorporar workflows o múltiples agentes al caso.

## Navegación Labs ↔ Reference

Cada implementación tiene una sección **Conocimientos previos** con enlaces a los README de los labs que aplica. [RAG con Vector Store real](rag-vector-store/README.md#conocimientos-previos) ya permite recorrer ambos niveles desde Labs 07–10.

Los README de Labs 07–10 enlazan la referencia ejecutable. Mientras una candidata siga planificada, no agregamos enlaces a proyectos inexistentes ni placeholders en todos los labs.

## Documentación obligatoria de cada implementación

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
