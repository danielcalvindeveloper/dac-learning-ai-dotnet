# Lectura opcional - Chunking

## Dividir para trabajar con porciones

Chunking convierte un texto en una secuencia de fragmentos. El tamaño limita cuánto contenido entra en cada uno. En este laboratorio contamos unidades UTF-16 del string, no bytes ni tokens.

El código normaliza saltos de línea y recorta extremos del texto completo. Después toma hasta `chunkSize` posiciones y avanza `chunkSize - overlap`. Se detiene al cubrir el final, aunque el último fragmento sea más corto.

## Ventajas y límites

Los caracteres hacen visible el mecanismo y no requieren modelos ni librerías. Es suficiente para entender tamaño, secuencia y solapamiento.

El corte puede dividir palabras, títulos u oraciones, incluso una pareja UTF-16 en contenido con ciertos símbolos. No preserva perfectamente Markdown ni significado. El ejemplo usa texto sencillo; no es un chunker de producción.

Texto vacío o solo espacios produce una lista vacía. Se valida el tamaño y el overlap antes de dividir. No se ajustan parámetros inválidos silenciosamente.

## Otras estrategias

- Por tokens: limita unidades de un tokenizador, que no equivalen a caracteres.
- Semántica: busca límites relacionados con el significado o estructura.

Son conceptos para estudiar después, no funcionalidades de este lab. Un fragmento compuesto solo por espacios interiores es posible con cortes arbitrarios; no eliminamos esos espacios porque modificaría los límites y el overlap. El documento de ejemplo no produce fragmentos de ese tipo.

[Volver al laboratorio](../README.md).
