# 03 - Roadmap de laboratorios

El recorrido principal conserva los Labs 00–17. Primero entendemos y experimentamos, después abstraemos cuando aparece una necesidad y finalmente integramos. El [enfoque pedagógico y la evolución del roadmap](04-enfoque-pedagogico-y-evolucion-del-roadmap.md) explican esa progresión.

Lab 00 es la rampa de entrada opcional. Los Labs 00–11 y M5 están implementados, incluyendo los sublabs 09a, 09b, 10a, 10b, 11a y 11b. M6 está en progreso y los Labs 12–17 continúan pendientes.

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
10. [Retrieval y calidad](../labs/lab-10-retrieval-calidad/README.md) — ✅ Implementado

Lab 10 tiene dos etapas implementadas: [10a - Múltiples documentos](../labs/lab-10-retrieval-calidad/lab-10a-multiples-documentos/README.md) pasa a un corpus conservando el origen de cada chunk; [10b - Relevancia, threshold y fuentes](../labs/lab-10-retrieval-calidad/lab-10b-relevancia-threshold-fuentes/README.md) distingue cantidad de resultados de contexto suficiente, muestra evidencia aceptada y evita generación cuando no queda contexto. El umbral es experimental y necesita calibración; no es universal ni un porcentaje de relevancia.

Este bloque ofrece una base práctica de RAG: embeddings → documentos → chunks → pipeline explícito → `RagService` → múltiples documentos → relevancia y fuentes. Las técnicas avanzadas quedan como posibles extensiones.

## Bloque 3 - Capacidades y automatización

11. [Tools](../labs/lab-11-tools/README.md) — ✅ Implementado mediante [11a - Primer Tool explícito](../labs/lab-11-tools/lab-11a-tool-explicita/README.md) y [11b - Invocación automática de Tools](../labs/lab-11-tools/lab-11b-tool-invocation/README.md). Ambos completaron el ciclo con Gemini, el caso que permitió observar la necesidad de conservar metadata nativa. La preservación depende de la representación recibida del SDK OpenAI, sin condiciones por proveedor; es explícita en 11a y queda en el cliente base en 11b.
12. [Function Calling](../labs/lab-12-function-calling/README.md) — ⏳ Pendiente: múltiples Tools, selección, argumentos, errores, casos sin Tool y una Tool que encapsule una capacidad externa. La invocación automática ya se estudia en 11b.
13. [Primer agente](../labs/lab-13-agents/README.md) — ⏳ Pendiente: objetivo, tools, estado/contexto y decisiones iterativas con límites.
14. [Workflow](../labs/lab-14-multi-agent/README.md) — ⏳ Pendiente: pasos y bifurcaciones explícitos para un proceso conocido.
15. [Multi-agent](../labs/lab-15-workflows/README.md) — ⏳ Pendiente: especialización y coordinación entre pocos agentes, evaluando su aporte.
16. [MCP](../labs/lab-16-mcp/README.md) — ⏳ Pendiente: exponer y consumir capacidades mediante un protocolo estándar; estructura por definir.

Tool calling no equivale a un agente. Un workflow controla explícitamente el proceso; más agentes no implica una mejor solución.

## Bloque 4 - Integración

17. [Proyecto final](../labs/lab-17-final-project/README.md) — ⏳ Pendiente: elegir capacidades según el problema, implementarlas y justificar las decisiones. No obliga a combinar todas las tecnologías estudiadas.

## Criterio de avance

Hasta Lab 06 `AI_MODEL` cubre las capacidades generativas/chat. Desde Lab 07 agregamos `AI_EMBEDDING_MODEL` para generar vectores, porque la compatibilidad con chat no implica soporte de embeddings. Esta distinción aparece cuando se necesita, siguiendo KISS.

Ambos modelos pueden coexistir y compartir `AI_PROVIDER`, `AI_API_KEY` y `AI_URL`. Lab 09a utiliza embeddings para recuperar contenido y chat para generar respuestas; Lab 09b encapsula ese mismo pipeline en un servicio. Lab 10a amplía la búsqueda a un corpus y 10b decide qué candidatos acepta antes de generar. Lab 08 carga y divide documentos locales sin configuración de IA. Lab 11 utiliza únicamente chat para solicitar una Tool local y recibir su resultado: primero explícitamente en 11a y después con invocación automática en 11b. M5 está implementado y M6 está en progreso; Labs 12 en adelante continúan pendientes. El estado detallado se encuentra en [ROADMAP.md](../ROADMAP.md).

Cada laboratorio debería contener, cuando se implemente:

- objetivo;
- concepto;
- problema que resuelve;
- implementación mínima;
- forma de ejecución;
- resultado esperado;
- relación con el laboratorio anterior.

Lab 09 comprende [09a - RAG explícito](../labs/lab-09-rag-basico/lab-09a-rag-explicito/README.md) y [09b - RAG con RagService](../labs/lab-09-rag-basico/lab-09b-rag-service/README.md). Primero comprendemos el pipeline y después encapsulamos su coordinación. `topK` ya está implementado; Lab 10a busca en múltiples documentos y Lab 10b muestra qué hacer cuando los mejores resultados no alcanzan el mínimo de similitud elegido.

Lab 10 utiliza su ruta definitiva `lab-10-retrieval-calidad/` con ambos sublabs. Lab 11 conserva la carpeta padre `lab-11-tools/`, titulada “Lab 11 - Tools”, y contiene `lab-11a-tool-explicita/` y `lab-11b-tool-invocation/`, cada uno con su proyecto y documentación. Los Labs 12–17 continúan pendientes, incluido `lab-12-function-calling/`, cuyo alcance es trabajar con múltiples Tools.

## Posibles extensiones futuras

Fuera del recorrido principal podrán estudiarse **RAG avanzado**, **Agents avanzado**, **MCP avanzado**, evaluación y observabilidad, u otras áreas que surjan. No tienen numeración ni estructura comprometida. El [roadmap](../ROADMAP.md#posibles-extensiones-futuras) enumera posibles técnicas de profundización en RAG.
