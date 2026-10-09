# 03 - Roadmap de laboratorios

El recorrido principal conserva los Labs 00–17. Primero entendemos y experimentamos, después abstraemos cuando aparece una necesidad y finalmente integramos. El [enfoque pedagógico y la evolución del roadmap](04-enfoque-pedagogico-y-evolucion-del-roadmap.md) explican esa progresión.

Lab 00 es la rampa de entrada opcional. Los Labs 00–09 y Lab 10a están implementados; Lab 10 está en progreso porque falta 10b. Los Labs 11–17 continúan pendientes.

## Bloque 1 - Fundamentos

1. Hello LLM
2. Chat History
3. Services y Dependency Injection
4. Prompt Templates
5. Structured Output
6. Memoria conversacional avanzada

## Bloque 2 - Conocimiento externo

7. [Embeddings](../labs/lab-07-embeddings/README.md) — ✅ Implementado
8. [Document Loading y Chunking](../labs/lab-08-document-loading/README.md) — ✅ Implementado
9. [RAG básico](../labs/lab-09-rag-basico/README.md) — ✅ Implementado mediante [Lab 09a - RAG explícito](../labs/lab-09-rag-basico/lab-09a-rag-explicito/README.md) y [Lab 09b - RAG con RagService](../labs/lab-09-rag-basico/lab-09b-rag-service/README.md)
10. [Retrieval y calidad](../labs/lab-10-retrieval-calidad/README.md) — 🚧 En progreso

Lab 10 tiene dos etapas: [10a - Múltiples documentos](../labs/lab-10-retrieval-calidad/lab-10a-multiples-documentos/README.md), **implementado**, pasa a un corpus conservando el origen de cada chunk; **10b - Relevancia, threshold y fuentes**, pendiente, distinguirá cantidad de resultados de contexto suficiente. Un umbral se calibra con el modelo, el corpus y las preguntas; no es universal.

Este bloque dará una base práctica de RAG: embeddings → documentos → chunks → pipeline explícito → `RagService` → múltiples documentos → relevancia y fuentes. Las técnicas avanzadas quedan como posibles extensiones.

## Bloque 3 - Capacidades y automatización

11. [Primer Tool](../labs/lab-11-tools/README.md) — ⏳ Pendiente: function calling, argumentos y ejecución real en .NET.
12. [Múltiples Tools](../labs/lab-12-function-calling/README.md) — ⏳ Pendiente: seleccionar herramientas, no usar ninguna cuando corresponda y manejar errores básicos.
13. [Primer agente](../labs/lab-13-agents/README.md) — ⏳ Pendiente: objetivo, tools, estado/contexto y decisiones iterativas con límites.
14. [Workflow](../labs/lab-14-multi-agent/README.md) — ⏳ Pendiente: pasos y bifurcaciones explícitos para un proceso conocido.
15. [Multi-agent](../labs/lab-15-workflows/README.md) — ⏳ Pendiente: especialización y coordinación entre pocos agentes, evaluando su aporte.
16. [MCP](../labs/lab-16-mcp/README.md) — ⏳ Pendiente: exponer y consumir capacidades mediante un protocolo estándar; estructura por definir.

Tool calling no equivale a un agente. Un workflow controla explícitamente el proceso; más agentes no implica una mejor solución.

## Bloque 4 - Integración

17. [Proyecto final](../labs/lab-17-final-project/README.md) — ⏳ Pendiente: elegir capacidades según el problema, implementarlas y justificar las decisiones. No obliga a combinar todas las tecnologías estudiadas.

## Criterio de avance

Hasta Lab 06 `AI_MODEL` cubre las capacidades generativas/chat. Desde Lab 07 agregamos `AI_EMBEDDING_MODEL` para generar vectores, porque la compatibilidad con chat no implica soporte de embeddings. Esta distinción aparece cuando se necesita, siguiendo KISS.

Ambos modelos pueden coexistir y compartir `AI_PROVIDER`, `AI_API_KEY` y `AI_URL`. Lab 09a utiliza embeddings para recuperar contenido y chat para generar respuestas; Lab 09b encapsula ese mismo pipeline en un servicio. Lab 10a amplía la búsqueda a un corpus manteniendo visibles las fuentes. Lab 08 carga y divide documentos locales sin configuración de IA. Lab 10b y Labs 11 en adelante continúan pendientes; M5 sigue en progreso. El estado detallado se encuentra en [ROADMAP.md](../ROADMAP.md).

Cada laboratorio debería contener, cuando se implemente:

- objetivo;
- concepto;
- problema que resuelve;
- implementación mínima;
- forma de ejecución;
- resultado esperado;
- relación con el laboratorio anterior.

Lab 09 comprende [09a - RAG explícito](../labs/lab-09-rag-basico/lab-09a-rag-explicito/README.md) y [09b - RAG con RagService](../labs/lab-09-rag-basico/lab-09b-rag-service/README.md). Primero comprendemos el pipeline y después encapsulamos su coordinación. `topK` ya está implementado; Lab 10a busca en múltiples documentos y Lab 10b trabajará qué hacer cuando los mejores resultados no sean suficientemente relevantes.

Lab 10 ya utiliza su ruta definitiva `lab-10-retrieval-calidad/` con el sublab 10a. La carpeta de 10b todavía no existe. Los Labs 11–17 conservan el esqueleto documental actual; sus rutas se adecuarán al alcance al implementarlos.

## Posibles extensiones futuras

Fuera del recorrido principal podrán estudiarse **RAG avanzado**, **Agents avanzado**, **MCP avanzado**, evaluación y observabilidad, u otras áreas que surjan. No tienen numeración ni estructura comprometida. El [roadmap](../ROADMAP.md#posibles-extensiones-futuras) enumera posibles técnicas de profundización en RAG.
