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

[Lab 09b](../../lab-09b-rag-service/README.md) encapsula este mismo mecanismo en un servicio. [Lab 10a](../../../lab-10-retrieval-calidad/lab-10a-multiples-documentos/README.md) amplía la búsqueda a múltiples documentos y muestra las fuentes recuperadas. [Lab 10b](../../../lab-10-retrieval-calidad/lab-10b-relevancia-threshold-fuentes/README.md) incorpora un threshold experimental y evita generación sin contexto aceptado. Filtros por metadata, re-ranking y otras técnicas de RAG avanzado quedan como posibles extensiones fuera del recorrido principal, según el [enfoque pedagógico](../../../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).

[Volver al laboratorio](../README.md).
