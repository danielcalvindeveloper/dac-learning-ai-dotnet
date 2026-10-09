# Lectura opcional - Corpus de documentos

Un corpus es el conjunto de documentos sobre el que buscamos. En Lab 09 había un archivo; en 10a hay cuatro, con temas diferentes y algunas relaciones entre ellos.

## Qué cambia en el programa

`Directory.GetFiles(documentsPath, "*.md")` descubre los archivos de la carpeta. `Array.Sort(documentPaths, StringComparer.Ordinal)` fija su orden de procesamiento.

```text
crear un store
    ↓
por cada documento: cargar → chunks → embeddings → agregar al store
    ↓
consultar el índice completo
```

El store queda fuera del `foreach`. Crear otro dentro del ciclo separaría los fragmentos por documento y cambiaría el ejercicio: aquí queremos comparar la pregunta con todo el corpus.

## Cantidades distintas

Cada documento produce varios chunks. El total del índice es la suma de esos fragmentos. Generamos un embedding por chunk, manteniendo la asociación posicional con el texto que lo originó. La consulta posterior produce un único embedding, independientemente de cuántos archivos indexamos.

El corpus se prepara nuevamente al ejecutar el programa. No hay persistencia ni actualizaciones incrementales. La pregunta de este paso es cómo ampliar el conjunto de conocimiento manteniendo el pipeline conocido.

[Volver al laboratorio](../README.md).
