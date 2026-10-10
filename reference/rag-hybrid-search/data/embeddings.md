# Embeddings y representación semántica

## Representar un texto

Un embedding representa un texto mediante un vector numérico. El modelo transforma la entrada en una secuencia de valores que permite comparar relaciones de significado. Los números individuales no se interpretan manualmente como palabras o categorías. La representación completa es la que resulta útil para trabajar con cercanía semántica entre textos.

La dimensión es la cantidad de componentes del vector. Depende del modelo y de su configuración, por lo que no conviene asumir un tamaño fijo en el programa. Dos representaciones que vamos a comparar deben pertenecer al mismo espacio compatible. Usar el mismo modelo para los fragmentos y para la consulta permite mantener coherente esa comparación dentro del ejercicio.

## Cercanía de significado

Dos textos pueden estar relacionados aunque no utilicen exactamente las mismas palabras. Una persona puede preguntar por una computadora portátil para desarrollar software y un documento puede hablar de una laptop para programación. La búsqueda semántica intenta representar esa relación mediante cercanía entre vectores, en lugar de exigir coincidencias literales entre todos los términos.

La cercanía no demuestra que ambos textos digan lo mismo ni que su contenido sea verdadero. También influyen el idioma, la redacción, la longitud de las entradas y las capacidades del modelo. Los embeddings ofrecen una señal de relación que la aplicación puede utilizar. Conviene observar los fragmentos recuperados para comprender qué información acompaña a esa señal numérica.

## Generación desde .NET

Los laboratorios utilizan `IEmbeddingGenerator<string, Embedding<float>>` para solicitar embeddings de texto. `GenerateAsync` recibe las entradas y devuelve una colección de `Embedding<float>`. Cada elemento expone su vector mediante `Vector`. El programa puede conservar ese vector junto con el texto que lo originó para compararlo más adelante con otras representaciones.

Si enviamos varios fragmentos en una operación, esperamos un resultado para cada entrada. La asociación posicional permite emparejar el fragmento de una posición con el embedding de esa misma posición. Comprobar que ambas cantidades coincidan evita construir un índice donde falte una representación o se pierda la correspondencia entre texto y vector.

## De documentos a fragmentos

Antes de generar embeddings de un documento podemos dividir su contenido en chunks. Cada fragmento representa una parte manejable del texto original. Un chunk conserva su contenido, la fuente y una posición dentro del documento. El embedding se genera a partir de ese contenido y luego se almacena asociado al fragmento que permitirá recuperar el texto.

El chunking por caracteres es una estrategia visible para aprender el mecanismo. Un tamaño máximo limita cada fragmento y el overlap repite parte del final anterior al comienzo del siguiente. Puede cortar palabras u oraciones. La representación vectorial se calcula sobre el texto recibido, de modo que los límites elegidos afectan la información disponible en cada embedding.

## Comparar una consulta

La pregunta también se transforma en un embedding. Para una consulta necesitamos una representación, aunque el corpus tenga muchos fragmentos indexados. Después comparamos el vector de la pregunta con los vectores de todos los chunks. El programa ordena los resultados por similitud y conserva una cantidad máxima mediante `topK`.

La similitud coseno es una forma de comparar esas representaciones. Ya conocemos su implementación en los laboratorios anteriores. Aquí importa su papel: producir una medida que permita ordenar candidatos. El índice en memoria conserva las asociaciones entre fragmentos y vectores; la comparación decide cuáles aparecen primero para una consulta determinada.

## Relación de los embeddings con RAG

En un sistema RAG, los embeddings pueden sostener la etapa de retrieval. Representamos los chunks de los documentos y la pregunta en el mismo espacio vectorial, comparamos su cercanía y recuperamos fragmentos relacionados. Después usamos el texto de esos fragmentos como contexto para que un modelo generativo construya una respuesta.

Los embeddings ayudan a encontrar contexto; no redactan la respuesta del sistema RAG. El modelo de chat recibe la pregunta junto con el contenido recuperado y genera texto. Son dos capacidades complementarias: una organiza la recuperación semántica y la otra produce una respuesta apoyada en la información seleccionada por la aplicación.

## Un corpus y un espacio compartido

Cuando hay varios documentos, los chunks de todos ellos pueden formar un único índice. No necesitamos un índice separado por archivo para comprender esta búsqueda. La consulta se compara contra el corpus completo y los mejores candidatos pueden pertenecer a fuentes diferentes. La procedencia se conserva con cada fragmento para observar de dónde salió el contenido recuperado.

Los índices de chunk pueden repetirse entre archivos. El fragmento cero de un documento no es el fragmento cero de otro. En un ejemplo pequeño, el nombre de la fuente junto con la posición permite distinguirlos. El vector sirve para comparar significado; el origen y el índice sirven para identificar el texto que acompañará al resultado.

## Límites que conviene observar

Seleccionar tres candidatos no implica que los tres sean adecuados para responder. El índice devuelve los mejores fragmentos disponibles, incluso cuando la pregunta está lejos del contenido del corpus. Tampoco garantiza diversidad de fuentes: varios chunks de un mismo archivo pueden ocupar todas las primeras posiciones. Es una consecuencia del ordenamiento por similitud.

El ejercicio permite observar estas limitaciones sin agregar otros mecanismos. Mostramos la fuente, la posición, la similitud y el contexto antes de generar la respuesta. Esa visibilidad ayuda a distinguir un problema de recuperación de un problema de redacción. La representación vectorial es una herramienta para seleccionar información, dentro de un flujo cuya calidad depende de todas sus etapas.
