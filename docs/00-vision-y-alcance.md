# 00 - Visión y alcance

## Visión

Construir un recorrido práctico, autónomo e incremental para aprender integración de IA generativa en .NET.

El repositorio debe permitir estudiar un concepto por vez y comprender qué problema resuelve antes de incorporar abstracciones adicionales.

Primero entender, después abstraer y finalmente integrar. Experimentar y observar limitaciones da sentido al paso siguiente, como muestra la evolución del pipeline explícito de Lab 09a a `RagService` en Lab 09b. Lab 11 aplica el mismo principio: 11a muestra el ciclo explícito de una Tool y 11b delega su coordinación a `FunctionInvokingChatClient`.

## Dos recorridos complementarios

El corazón del proyecto sigue siendo el aprendizaje incremental: comprender fundamentos y observar sus mecanismos en `labs/`. [`reference/` — Implementaciones de referencia](../reference/README.md) agrega un nivel aplicado para experimentar con librerías, frameworks e infraestructura más cercanos a una aplicación real.

```text
comprender fundamentos + experimentar implementaciones aplicadas
```

Ambos niveles se relacionan: primero comprender el mecanismo, después estudiar una implementación aplicada y volver sobre las decisiones de cada una. Las referencias reducen esa distancia sin reemplazar los labs ni tratarlos como implementaciones incorrectas.

Las referencias [RAG con Vector Store real](../reference/rag-vector-store/README.md) y [RAG Hybrid Search + BM25](../reference/rag-hybrid-search/README.md) ya aplican Labs 07–10 con Qdrant persistente. Hybrid integra Lucene.NET para BM25, RRF propio y filtros nativos en ambos motores. Semantic Chunking y el agente con Microsoft Agent Framework siguen planificados.

## Principios

1. Un concepto principal por laboratorio.
2. Evitar complejidad accidental.
3. Mantener ejemplos pequeños.
4. Priorizar APIs idiomáticas de .NET.
5. Separar el concepto del proveedor de LLM.
6. Introducir cada distinción cuando aparezca la necesidad de utilizarla.
7. Incorporar frameworks de mayor nivel solamente cuando aporten valor.
8. Relacionar la comprensión conceptual con implementaciones aplicadas, eligiendo primero el problema y después la herramienta.

## Límites del recorrido conceptual

- aplicaciones completas de producción;
- interfaces gráficas;
- despliegue cloud;
- observabilidad avanzada;
- bases vectoriales externas;
- arquitecturas distribuidas.

Los labs limitan deliberadamente esa infraestructura. La primera implementación de `reference/` incorpora persistencia y un vector store real porque el caso lo justifica. Realista no significa industrial: se conservan ejemplos comprensibles y se evita una arquitectura corporativa completa.

El recorrido principal conserva los Labs 00–17. El bloque RAG llegará hasta retrieval, múltiples documentos, relevancia y fuentes; RAG avanzado será una posible extensión. Después avanzaremos desde una Tool explícita en 11a hacia su invocación automática en 11b. Lab 12, Function Calling, ya implementa múltiples Tools en 12a, decisión y errores en 12b y una capacidad HTTP externa en 12c; Lab 13 ya implementa un primer agente de soporte, con objetivo, estado, observaciones y decisiones iterativas limitadas. Luego seguiremos con workflow, multi-agent, MCP e integración según el problema.

Microsoft.Extensions.AI es la base de integración con modelos y abstracciones AI. Microsoft.Extensions.VectorData permite incorporar infraestructura vectorial especializada en RAG. Microsoft Agent Framework es el framework objetivo para nuevos escenarios agénticos después de comprender el loop explícito de Lab 13; complementa las abstracciones base. Primero el problema, después la herramienta.

El [documento de enfoque pedagógico y evolución del roadmap](04-enfoque-pedagogico-y-evolucion-del-roadmap.md) explica el fundamento. [ROADMAP.md](../ROADMAP.md) distingue planificación de implementación.
