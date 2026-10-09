# 00 - Visión y alcance

## Visión

Construir un recorrido práctico, autónomo e incremental para aprender integración de IA generativa en .NET.

El repositorio debe permitir estudiar un concepto por vez y comprender qué problema resuelve antes de incorporar abstracciones adicionales.

Primero entender, después abstraer y finalmente integrar. Experimentar y observar limitaciones da sentido al paso siguiente, como muestra la evolución del pipeline explícito de Lab 09a a `RagService` en Lab 09b.

## Principios

1. Un concepto principal por laboratorio.
2. Evitar complejidad accidental.
3. Mantener ejemplos pequeños.
4. Priorizar APIs idiomáticas de .NET.
5. Separar el concepto del proveedor de LLM.
6. Introducir cada distinción cuando aparezca la necesidad de utilizarla.
7. Incorporar frameworks de mayor nivel solamente cuando aporten valor.

## Fuera de alcance inicial

- aplicaciones completas de producción;
- interfaces gráficas;
- despliegue cloud;
- observabilidad avanzada;
- bases vectoriales externas;
- arquitecturas distribuidas.

Estos temas podrán incorporarse posteriormente si resultan útiles para el aprendizaje.

El recorrido principal conserva los Labs 00–17. El bloque RAG llegará hasta retrieval, múltiples documentos, relevancia y fuentes; RAG avanzado será una posible extensión. Después avanzaremos desde una tool hacia múltiples tools, un agente, workflow, multi-agent, MCP e integración según el problema.

El [documento de enfoque pedagógico y evolución del roadmap](04-enfoque-pedagogico-y-evolucion-del-roadmap.md) explica el fundamento. [ROADMAP.md](../ROADMAP.md) distingue planificación de implementación.
