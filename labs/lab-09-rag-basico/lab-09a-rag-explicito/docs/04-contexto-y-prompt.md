# Lectura opcional - Contexto y prompt

```text
chunks recuperados → contexto → prompt → modelo generativo
```

`Program.cs` reúne el contenido de los resultados en orden de similitud. Cada fragmento mantiene un encabezado:

```text
[Fuente: guia-dotnet-ai.md - Chunk <índice>]
<contenido>
```

`Source` identifica el archivo e `Index` su posición en la secuencia de chunks, comenzando en cero. No es una página, un ID persistente ni un desplazamiento exacto de caracteres. Mostrar ambos ayuda a inspeccionar el material recuperado.

`PromptTemplates.Rag(context, question)` inserta esos valores en una instrucción sencilla: responder utilizando únicamente el contexto e indicar si falta información suficiente. El prompt se construye fuera del código que coordina las etapas.

Después `IChatClient.GetResponseAsync` envía el prompt al modelo de `AI_MODEL`. Este es el momento de generación; los embeddings ya cumplieron su función de recuperación.

Pedir que se use solo el contexto reduce el riesgo de respuestas externas, pero no garantiza ausencia de alucinaciones ni cumplimiento perfecto de la instrucción. Los encabezados muestran procedencia; no implementamos citaciones formales ni verificación automática de la respuesta.

[Volver al laboratorio](../README.md).
