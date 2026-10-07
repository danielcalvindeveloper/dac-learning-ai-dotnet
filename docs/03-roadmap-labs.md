# 03 - Roadmap de laboratorios

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
9. RAG básico — ⏳ Pendiente
10. RAG avanzado — ⏳ Pendiente

## Bloque 3 - Capacidades y automatización

11. Tools
12. Function Calling
13. Agents
14. Multi-Agent
15. Workflows
16. MCP

## Bloque 4 - Integración

17. Proyecto final

## Criterio de avance

Hasta Lab 06 `AI_MODEL` cubre las capacidades generativas/chat. Desde Lab 07 agregamos `AI_EMBEDDING_MODEL` para generar vectores, porque la compatibilidad con chat no implica soporte de embeddings. Esta distinción aparece cuando se necesita, siguiendo KISS.

Ambos modelos pueden coexistir y compartir `AI_PROVIDER`, `AI_API_KEY` y `AI_URL`. Los futuros labs de RAG utilizarán embeddings para recuperar contenido y chat para generar respuestas. Lab 08 carga y divide documentos locales sin configuración de IA; Labs 09 en adelante continúan pendientes; el estado detallado se encuentra en [ROADMAP.md](../ROADMAP.md).

Cada laboratorio debería contener, cuando se implemente:

- objetivo;
- concepto;
- problema que resuelve;
- implementación mínima;
- forma de ejecución;
- resultado esperado;
- relación con el laboratorio anterior.

Los Labs 00-08 están implementados; los siguientes conservan su estructura inicial hasta que se desarrollen.
