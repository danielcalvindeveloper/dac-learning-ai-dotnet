# Lectura opcional - Qué es RAG

**Retrieval-Augmented Generation** reúne tres acciones:

- Retrieval: recuperar información relacionada con una pregunta;
- Augmented: agregar esa información como contexto;
- Generation: generar una respuesta utilizando la pregunta y el contexto.

Sin recuperación:

```text
pregunta → modelo generativo → respuesta
```

Con RAG:

```text
pregunta → recuperar información → contexto + pregunta → modelo generativo → respuesta
```

RAG no entrena el modelo. Proporciona contexto durante la consulta. El modelo tampoco abre nuestros archivos automáticamente: la aplicación carga y prepara los documentos y decide qué fragmentos enviar.

En este ejemplo podemos inspeccionar los fragmentos antes de leer la respuesta. Esa observación permite distinguir un problema de recuperación de uno de generación, aunque no implementamos evaluación automática.

[Volver al laboratorio](../README.md).
