# Lab 09 - RAG básico

## Objetivo

Comprender y luego encapsular un pipeline RAG: recuperar fragmentos de documentos y utilizarlos como contexto para generar una respuesta.

## Sublaboratorios

| Sublaboratorio | Foco | Estado |
|---|---|---|
| [Lab 09a - RAG explícito](lab-09a-rag-explicito/README.md) | Ver el pipeline completo paso a paso | ✅ Implementado |
| [Lab 09b - RAG con RagService](lab-09b-rag-service/README.md) | Encapsular ese mismo pipeline en un servicio | ✅ Implementado |

Lab 09 está **implementado mediante 09a y 09b**. Primero comprendemos el mecanismo explícito; después encapsulamos ese mismo pipeline conocido en un servicio de aplicación.

```text
09a: Program.cs coordina todo
  ↓
09b: RagService coordina el pipeline
```

09b introduce `IRagService`, `RagService` y un resultado que conserva respuesta y chunks recuperados. Reutiliza DI como mecanismo de composición, sin agregar capacidades nuevas de RAG.

## Continuidad

Lab 07 introduce embeddings y similitud. Lab 08 prepara documentos como chunks. Lab 09a reúne esas capacidades y agrega generación con contexto recuperado.

Cada sublab se ejecuta independientemente y tiene sus propias instrucciones de configuración, ejecución y lecturas opcionales: [Lab 09a](lab-09a-rag-explicito/README.md) y [Lab 09b](lab-09b-rag-service/README.md). [Lab 10 - Retrieval y calidad](../lab-10-retrieval-calidad/README.md) está implementado: 10a trabaja múltiples documentos y 10b agrega un threshold experimental y fuentes del contexto aceptado.
