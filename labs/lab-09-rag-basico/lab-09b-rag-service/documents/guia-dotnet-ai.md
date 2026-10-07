# Guía de conceptos de IA en .NET

## Una aplicación tiene varias responsabilidades

Una aplicación con inteligencia artificial combina código convencional, datos y capacidades de modelos. Leer archivos, validar entradas, preparar mensajes y mostrar resultados siguen siendo responsabilidades del software. El modelo participa en tareas concretas, como generar una respuesta o representar un texto mediante un vector. Separar esas tareas ayuda a comprender qué ocurre cuando algo falla y evita atribuir al modelo capacidades que la aplicación nunca le proporcionó.

Esta guía presenta un recorrido pequeño: primero identificar las abstracciones, luego organizar instrucciones y datos, y finalmente combinar preparación de documentos, recuperación y generación. Los conceptos se complementan. Incorporar embeddings no reemplaza el chat, y dividir documentos no constituye por sí solo un sistema de preguntas y respuestas. Cada mecanismo resuelve una necesidad distinta dentro de una aplicación.

## Microsoft.Extensions.AI

Microsoft.Extensions.AI ofrece abstracciones comunes para trabajar con capacidades de inteligencia artificial desde .NET. Permite escribir parte de la aplicación contra contratos conocidos y dejar los detalles del proveedor en adaptadores. Una abstracción de chat representa conversaciones y respuestas; una abstracción de embeddings representa la transformación de entradas en vectores. La separación importa porque no todos los modelos admiten las mismas operaciones.

Trabajar con una abstracción no elimina las diferencias entre proveedores. La aplicación todavía debe configurar credenciales, modelos y un endpoint compatible. Algunas capacidades dependen del modelo seleccionado. Por eso conviene mantener la construcción de clientes fuera del flujo pedagógico principal y comprobar qué operación soporta cada servicio. Cambiar un nombre de modelo no transforma automáticamente un cliente de chat en un generador de embeddings.

## IChatClient

IChatClient es la abstracción de Microsoft.Extensions.AI para enviar mensajes a un modelo de chat y obtener respuestas. Permite que el código consumidor utilice un contrato común sin depender directamente del SDK concreto del proveedor. Un adaptador conecta ese contrato con el cliente real. IChatClient representa la capacidad de conversación y generación; no almacena por sí solo los documentos de la aplicación ni realiza automáticamente recuperación semántica.

La aplicación puede llamar a GetResponseAsync y trabajar con un ChatResponse. Los mensajes describen la entrada y la respuesta contiene los mensajes generados. Si necesitamos contexto de documentos, debemos recuperarlo y enviarlo explícitamente. Si necesitamos historial conversacional, debemos conservarlo y reenviarlo según el diseño elegido. El contrato facilita la integración, pero no decide por nosotros qué información recibe el modelo.

## Dependency Injection

Dependency Injection permite proporcionar las dependencias de una clase desde el exterior. Un servicio puede recibir un IChatClient por constructor en lugar de construir un cliente concreto dentro de cada método. Esto separa la lógica de aplicación de la creación de sus colaboradores. El contenedor de .NET puede registrar implementaciones y resolver objetos cuando el tamaño de la aplicación justifica esa composición.

El ciclo de vida expresa cuánto dura una instancia. Un servicio transitorio se crea en cada resolución; una instancia compartida puede conservar estado durante la vida del proceso. Elegir un ciclo de vida requiere comprender dónde vive el estado y quién lo utiliza. Sin embargo, un ejemplo pequeño puede construir objetos directamente. Haber estudiado Dependency Injection no obliga a utilizar un contenedor cuando oculta el mecanismo que estamos aprendiendo.

## Prompt Templates

Un Prompt Template separa una instrucción reutilizable de los valores concretos que cambian en cada ejecución. Puede recibir un idioma, un texto para resumir o una pregunta acompañada por contexto. En C# una función que devuelve un string interpolado puede ser suficiente. No hace falta incorporar un motor de plantillas para observar cómo se construye una instrucción parametrizada y mantenerla fuera del código que coordina el ejercicio.

En un ejemplo de recuperación, el template recibe contexto y pregunta. La instrucción pide responder con el material proporcionado e indicar cuando falta información. Esta regla orienta la generación y facilita revisar el resultado. No demuestra que la respuesta sea correcta ni impide todos los errores del modelo. El contexto sigue siendo contenido que la aplicación debe seleccionar, preparar y comprobar antes de enviarlo.

## Structured Output

Structured Output busca recibir datos compatibles con una estructura conocida, en lugar de depender exclusivamente de texto libre. Una aplicación puede necesitar un nombre, una edad o una categoría como propiedades de un objeto C#. Definir el contrato esperado facilita que el código use esos datos sin intentar extraerlos de una oración redactada de maneras diferentes en cada respuesta.

Solicitar una estructura no reemplaza la validación. La aplicación debe comprobar que pudo obtener un resultado compatible y luego aplicar las reglas que correspondan al dominio. Un dato puede tener el tipo correcto y aun así ser incorrecto. Los contratos describen la forma; las reglas de negocio describen condiciones adicionales. En un laboratorio de RAG básico podemos seguir utilizando texto como respuesta, porque la capacidad nueva está en recuperar contexto, no en introducir otro formato de salida.

## Memoria conversacional

La memoria conversacional conserva mensajes anteriores para que una nueva interacción pueda utilizar información de la conversación. Un historial es una colección de mensajes que la aplicación vuelve a enviar al modelo. Si existen varias conversaciones, cada sesión necesita su propio historial para evitar mezclar usuarios o temas. El identificador de sesión permite seleccionar la secuencia correspondiente antes de construir la entrada de la siguiente llamada.

Conservar mensajes en memoria del proceso no implica persistencia. Al cerrar la aplicación, ese estado puede desaparecer. Una ventana limitada ayuda a controlar cuánto historial se mantiene, pero también descarta información antigua. La memoria conversacional y la recuperación documental resuelven necesidades distintas: una aporta intercambios previos; la otra selecciona contenido de documentos. Pueden combinarse más adelante, aunque no es necesario hacerlo para entender el primer pipeline de recuperación y generación.

## Embeddings

Un embedding representa un texto mediante una secuencia de números. Un modelo preparado para esa tarea aprende representaciones en las que textos relacionados pueden tener vectores cercanos. El vector no es una respuesta escrita y sus componentes individuales no suelen interpretarse manualmente. Para comparar representaciones, los textos deben utilizar el mismo modelo de embeddings y una configuración compatible; tener igual dimensión no garantiza pertenecer al mismo espacio.

IEmbeddingGenerator expresa la capacidad de generar embeddings. En un índice documental asociamos cada fragmento con su vector y conservamos el contenido original para recuperarlo después. La pregunta también se transforma en un vector del mismo espacio. Una medida como similitud coseno permite ordenar los fragmentos según su cercanía con la consulta. Ese score no es una probabilidad de respuesta correcta y no prueba que un fragmento contenga toda la información necesaria.

## Chunking

Chunking divide un texto en fragmentos más pequeños. Antes de dividir, una normalización mínima puede unificar saltos de línea y recortar los extremos del documento. Un método por caracteres toma hasta cierta cantidad de posiciones y avanza una distancia definida. Es visible, fácil de implementar y suficiente para comprender tamaño, secuencia y solapamiento. También puede cortar palabras u oraciones, una limitación importante al interpretar los resultados recuperados.

El overlap repite parte del final de un fragmento al comienzo del siguiente. Reduce el efecto de un corte arbitrario y aumenta redundancia; no comprende la semántica ni garantiza conservar una idea completa. Cada chunk mantiene contenido, origen y posición en la secuencia. Esos datos permiten volver al material que se utilizó. No existe un tamaño universal perfecto: el documento, la tarea y las capacidades posteriores influyen en la elección.

## RAG: recuperar y generar

RAG significa Retrieval-Augmented Generation. Combina recuperación de información con generación apoyada en contexto. Durante la indexación, la aplicación carga documentos, genera chunks, obtiene embeddings y guarda la asociación entre cada fragmento y su vector. Esta etapa puede realizarse antes de recibir preguntas. Un almacén en memoria basta para aprender el mecanismo; al terminar el proceso, el índice se pierde si no existe persistencia.

Durante la consulta, la aplicación genera el embedding de la pregunta y lo compara con los vectores indexados. Recupera como máximo topK fragmentos, construye un contexto con su texto y lo incorpora al prompt junto con la pregunta. Finalmente llama al modelo generativo para obtener una respuesta. RAG no entrena el modelo: le proporciona información durante la consulta. La calidad depende de los documentos, de los fragmentos recuperados y de cómo el modelo utiliza ese contexto.
