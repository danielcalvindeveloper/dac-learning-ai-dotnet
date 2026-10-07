# Lectura opcional - Limitaciones del RAG básico

Las limitaciones son decisiones pedagógicas deliberadas para observar el pipeline:

- Chunking por caracteres: puede cortar palabras, oraciones y unidades UTF-16 de ciertos símbolos. Overlap conserva una parte literal, sin comprender el texto.
- Store en RAM y sin persistencia: el índice desaparece al cerrar el proceso.
- Búsqueda lineal: cada consulta compara todos los vectores, suficiente para este documento pequeño.
- TopK fijo: limita resultados, pero no garantiza que sean relevantes.
- Sin threshold, filtros ni re-ranking: no descartamos por score mínimo ni refinamos el orden.
- Sin evaluación automática: inspeccionamos contexto y respuesta, sin medir su calidad mediante un sistema adicional.
- Sin citaciones formales: mostramos origen e índice del contexto, sin verificar citas en la respuesta.

Una recuperación incorrecta o incompleta puede producir una respuesta insuficiente. El modelo generativo también puede cometer errores aun con fragmentos adecuados. Un score alto no demuestra que el contenido sea verdadero.

No incorporamos hybrid search, query rewriting, agentes ni gestión avanzada del contexto. Las consultas requieren servicios externos y pueden fallar por credenciales, modelos incompatibles o límites del proveedor.

[Lab 09b](../../lab-09b-rag-service/README.md) encapsula este mismo mecanismo en un servicio. Las mejoras de recuperación corresponden a Lab 10, que continúa pendiente.

[Volver al laboratorio](../README.md).
