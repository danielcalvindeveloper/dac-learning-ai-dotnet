# Lectura opcional - Threshold experimental

`minimumSimilarity = 0.70` es una decisión del experimento. El filtrado acepta valores mayores o iguales al mínimo, conservando la precisión calculada; el formato de cuatro decimales es solo para mostrar el score.

## No es un porcentaje

La similitud compara embeddings. Un valor de 0.70 no mide una probabilidad ni significa 70 % de relevancia. Su interpretación depende del modelo, corpus, preguntas, chunking, distribución de scores y dominio.

## Observar antes de ajustar

En la prueba con `gemini-embedding-001`, el máximo de la consulta ajena al corpus quedó aproximadamente en 0.5317 y los candidatos relacionados superaron 0.84. El mínimo elegido separó ambos ejemplos. Esa observación no demuestra que funcione para todas las preguntas, ni para otro modelo.

Podés cambiar la constante en `Program.cs` y volver a ejecutar para observar qué se acepta y qué se descarta. Un mínimo alto puede perder evidencia útil y uno bajo admitir contenido ajeno. No existe un valor universal y el lab no automatiza su calibración.

El parámetro permanece en el código porque es el objeto del experimento. La configuración de proveedor, modelos y credencial mantiene su responsabilidad habitual.

[Volver al laboratorio](../README.md).
