# Introducción a la inteligencia artificial generativa

## Una forma de trabajar con información

La inteligencia artificial generativa permite producir contenido a partir de instrucciones y ejemplos. Puede ayudar a redactar, resumir, organizar información o explorar alternativas. Su utilidad depende de la tarea, de los datos disponibles y de la revisión de los resultados. No es una fuente infalible de conocimiento ni reemplaza automáticamente el criterio de una persona. Conviene aprender sus mecanismos básicos antes de construir una aplicación que dependa de ellos.

En una aplicación, el modelo es una pieza entre varias. También existen entradas, reglas de negocio, documentos, validaciones y una interfaz para mostrar resultados. Separar estas responsabilidades ayuda a observar qué sucede en cada etapa. Un problema de lectura de archivos no se resuelve cambiando el modelo, del mismo modo que un documento incompleto no mejora por agregar una instrucción más extensa.

## Modelos de lenguaje

Un modelo de lenguaje procesa secuencias de texto y genera continuaciones según los patrones aprendidos durante su entrenamiento. En una conversación puede responder preguntas o reformular instrucciones. Sin embargo, una respuesta fluida puede contener errores, omisiones o afirmaciones que no estén respaldadas. La apariencia de seguridad no debe confundirse con exactitud. Por eso es importante revisar las respuestas cuando se utilizan para tomar decisiones.

El modelo recibe un contexto limitado. Ese contexto puede incluir instrucciones, mensajes anteriores y material de referencia. No debemos suponer que conoce los archivos de nuestra computadora o los documentos privados de una organización. Para trabajar con contenido local, una aplicación debe leerlo y decidir cómo incorporarlo al procesamiento. Esa preparación es una tarea de software independiente de la generación de respuestas.

## Prompts e instrucciones

Un prompt expresa lo que esperamos del modelo. Puede describir una tarea, indicar un formato o proporcionar ejemplos. Una instrucción clara ayuda a reducir ambigüedades, pero no elimina todos los errores. Por ejemplo, pedir un resumen con tres ideas principales define mejor el resultado esperado que solicitar simplemente una opinión sobre un documento sin explicar qué información interesa.

También es útil distinguir instrucciones y datos. Un manual que se entrega como referencia puede contener ejemplos de comandos o mensajes destinados a otras personas. Esos textos no deberían convertirse automáticamente en nuevas instrucciones de la aplicación. La forma de presentar el material y las validaciones posteriores son parte del diseño. La calidad del resultado depende tanto de ese diseño como del modelo utilizado.

## Documentos y texto

Un documento puede estar guardado como archivo de texto o Markdown. Antes de procesarlo necesitamos acceder a su contenido. La ruta identifica dónde encontrar el archivo; la lectura devuelve una representación del texto en memoria. Son cosas diferentes: conocer el nombre del archivo no significa disponer de sus párrafos. Además, los bytes deben interpretarse con una codificación adecuada para conservar correctamente los caracteres.

Una normalización mínima permite trabajar con saltos de línea consistentes y quitar espacios en los extremos del contenido. No es necesario borrar títulos, convertir todo a minúsculas o eliminar palabras para comenzar. Es preferible conservar la información y hacer visibles las transformaciones. Si una limpieza cambia demasiado el documento, después será más difícil explicar de dónde provino un fragmento o por qué se perdió una expresión.

## Fragmentos de contenido

Un documento largo puede dividirse en fragmentos más pequeños, también llamados chunks. Una estrategia sencilla toma una cantidad máxima de caracteres en cada paso. Esto permite observar el tamaño y la secuencia sin depender de herramientas adicionales. El último fragmento puede contener menos caracteres. Un corte arbitrario también puede separar una palabra o una oración, y esa limitación debe reconocerse.

El solapamiento repite parte del final de un fragmento al comienzo del siguiente. Su propósito es reducir la pérdida de contexto causada por el corte. No comprende el significado del texto ni garantiza que una idea quede completa. Además, agrega redundancia: algunas partes aparecen varias veces. Elegir tamaño y solapamiento requiere considerar el tipo de documento y la tarea posterior, no aplicar una cifra universal.

## Embeddings y representaciones

Un embedding representa una entrada mediante una secuencia de números. Modelos preparados para esa tarea pueden ubicar textos relacionados en regiones cercanas de un espacio vectorial. La representación sirve para comparar contenidos, pero no es una respuesta redactada. Sus valores individuales no suelen tener una interpretación manual sencilla. El modelo y su configuración determinan cómo se produce esa representación.

Generar embeddings es una etapa distinta de cargar archivos. Primero necesitamos disponer del texto que queremos representar. Si dividimos documentos, cada fragmento podrá procesarse por separado en otro ejercicio. Conservar su origen ayuda a asociar la representación con el contenido original. En este documento nombramos esa posibilidad para comprender la progresión, sin ejecutar ninguna llamada a un modelo.

## Recuperación de información y RAG

Recuperar información consiste en seleccionar contenido que pueda ser relevante para una consulta. Existen estrategias basadas en palabras y otras basadas en representaciones vectoriales. Sus resultados deben evaluarse: una coincidencia no demuestra que el fragmento contenga la respuesta completa. Conservar referencias al documento de origen permite revisar el material y comprender los límites de lo recuperado.

RAG combina recuperación con generación de respuestas apoyadas en contexto. Preparar documentos es una parte previa que puede utilizarse en ese recorrido, pero dividir texto no constituye por sí solo un sistema RAG. En este laboratorio el trabajo termina al obtener fragmentos con contenido, origen y posición. Observar esa transformación permite aprender un mecanismo concreto antes de agregar modelos, búsquedas y nuevas responsabilidades.
