# Lectura opcional - Document Loading

## Archivo y contenido

Un archivo tiene una ruta en disco. Cargarlo significa leer sus bytes e interpretarlos como texto; el resultado del laboratorio es un `string` en memoria.

`DocumentLoader.LoadAsync` primero comprueba existencia y extensión. Luego usa `File.ReadAllTextAsync`: lee el contenido completo y permite esperar la operación con `await`.

## Por qué TXT y Markdown

Ambos se pueden leer como texto plano. Leer un archivo Markdown conserva sus títulos, listas y símbolos; no lo transforma en HTML ni elimina su sintaxis.

PDF y Word requieren interpretar otros formatos. Incorporarlos ahora ocultaría el mecanismo de archivo → texto detrás de tareas de parsing.

## Encoding

La codificación determina cómo los bytes representan caracteres. Conviene guardar el ejemplo en UTF-8. La API utiliza UTF-8 por defecto y reconoce marcas de codificación compatibles; no adivina todas las codificaciones antiguas.

Una extensión correcta no garantiza una codificación correcta. Este laboratorio no implementa detección avanzada.

[Volver al laboratorio](../README.md).
