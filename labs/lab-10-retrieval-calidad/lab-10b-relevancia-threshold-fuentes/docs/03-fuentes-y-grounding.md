# Lectura opcional - Fuentes y grounding

En 10a, `Source` permite reconocer el archivo de origen. En 10b, los chunks aceptados son también el material que enviamos como evidencia al modelo generativo.

## Una colección para contexto y fuentes

```text
relevantResults
    ├── encabezados Source + Index y contenido → prompt
    └── Source + Index + Similarity → fuentes mostradas
```

La aplicación conoce esos resultados. No le pide al modelo que reconstruya la lista ni que invente referencias bibliográficas. Las fuentes recuperadas son distintas de las citas que podría generar un LLM.

Si no hay resultados aceptados, el programa termina esa consulta antes de construir contexto o llamar al modelo. El embedding de la pregunta ya se generó para buscar; la llamada que se evita es la generación de texto.

Si hay contexto, el prompt conserva la instrucción de responder solo con él e indicar si resulta insuficiente. Un fragmento puede superar el threshold sin contener toda la respuesta. Grounding orienta la generación, pero no garantiza que cada afirmación sea correcta.

“Fuentes utilizadas” identifica el contexto enviado, no una verificación de citas o de uso individual de cada fragmento en la respuesta.

[Volver al laboratorio](../README.md).
