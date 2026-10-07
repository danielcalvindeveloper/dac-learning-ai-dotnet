# Lectura opcional - DocumentChunk

## Más que un string anónimo

`List<string>` conserva texto, pero no expresa por sí sola de qué archivo vino cada fragmento ni su posición en la secuencia del documento.

El modelo agrega únicamente:

| Propiedad | Ejemplo | Sentido |
|---|---|---|
| Content | Un párrafo o una porción de él | Contenido |
| Source | introduccion-ia.md | Origen |
| Index | 0, 1, 2… | Posición en la secuencia |

Cada archivo puede comenzar otra secuencia en 0. Index no es un ID persistente, un número de página ni una posición exacta en caracteres. Source es aquí el nombre del archivo; en este lab de un solo documento es suficiente.

Si después reunimos muchos documentos con nombres repetidos, habrá que revisar cómo identificar su origen. No resolvemos esa necesidad futura agregando complejidad ahora.

Los chunks se crean en memoria y se muestran. No tienen vectores, puntuaciones, fechas ni diccionarios de metadata.

[Volver al laboratorio](../README.md).
