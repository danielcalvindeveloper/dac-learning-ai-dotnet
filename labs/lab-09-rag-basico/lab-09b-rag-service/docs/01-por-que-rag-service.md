# Lectura opcional - Por qué RagService

En Lab 09a `Program.cs` coordinaba carga, chunking, embeddings, índice, recuperación, contexto, prompt y generación. Ver cada etapa era el objetivo del ejercicio.

Una vez comprendido ese mecanismo, aparece una responsabilidad concreta: ofrecer a un consumidor la capacidad de indexar documentos y consultar conocimiento. Un servicio de aplicación reúne la coordinación necesaria para realizar esas tareas.

**La abstracción aparece después de comprender el mecanismo que está encapsulando.**

En Lab 09b esa responsabilidad vive en `RagService`. `IRagService` la expresa mediante `IndexDocumentAsync` y `AskAsync`. El consumidor puede utilizarla sin repetir toda la orquestación.

La interfaz retoma contrato + implementación de Lab 03 y tiene aquí un propósito pedagógico concreto. No implica crear interfaces para cada componente: el store conserva una única implementación sencilla.

El pipeline puede reutilizarse para otras consultas mediante el mismo contrato. Esta evolución organiza el código; no agrega estrategias de RAG.

[Volver al laboratorio](../README.md).
