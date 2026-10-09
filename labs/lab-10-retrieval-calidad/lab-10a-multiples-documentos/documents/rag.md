# RAG y contexto externo

## Recuperar antes de generar

RAG combina recuperación de información con generación de una respuesta. La aplicación selecciona fragmentos de un corpus y los agrega como contexto a la pregunta que envía al modelo. El objetivo es que la respuesta pueda apoyarse en contenido externo elegido para esa consulta, en lugar de depender solamente de la información general que el modelo adquirió durante su entrenamiento.

El flujo no entrena nuevamente al modelo ni le concede acceso automático a una carpeta. La aplicación lee los archivos, prepara fragmentos y decide cuáles enviar. El modelo generativo ve la pregunta y el contexto incluido en la solicitud. Si un documento no fue cargado, indexado o recuperado, su contenido no aparece por ese mecanismo en el prompt.

## Dos momentos del proceso

La indexación prepara el conocimiento disponible. Primero se cargan documentos y se obtiene su texto. Después se normaliza y se divide en chunks. Cada fragmento conserva su fuente y posición. En el ejemplo básico se genera una representación vectorial para cada chunk y se almacena la asociación entre contenido, procedencia y vector en un índice en memoria.

La consulta utiliza ese índice. La pregunta se representa mediante un vector, se compara con los fragmentos almacenados y se seleccionan candidatos. Sus textos se reúnen en un contexto. Un template combina instrucciones, contexto y pregunta. Finalmente el modelo generativo produce una respuesta que la aplicación muestra junto con información sobre los fragmentos recuperados.

## Cómo se relaciona RAG con los embeddings

Los embeddings representan semánticamente los fragmentos y la pregunta para realizar retrieval en un sistema RAG. Al comparar vectores obtenidos con el mismo modelo, la aplicación encuentra chunks cercanos al significado de la consulta. Esos textos recuperados aportan contexto externo al modelo de chat, que genera la respuesta final.

Así, los embeddings resuelven una parte de la relación entre pregunta y conocimiento documental. El vector de la consulta permite ordenar fragmentos del corpus según cercanía semántica. La generación usa después el texto original de los candidatos seleccionados. El modelo de embeddings produce vectores y el modelo generativo produce texto; sus responsabilidades dentro del sistema son distintas.

## Retrieval sobre el corpus

Un corpus es el conjunto de documentos disponibles para el ejercicio. Puede contener una guía de interfaces, una explicación de representación vectorial, una descripción del pipeline y un texto sobre organización de dependencias. Los temas pueden relacionarse sin ser idénticos. Una pregunta que atraviese dos asuntos puede encontrar información útil en archivos diferentes.

La búsqueda opera sobre chunks de todo el corpus. No elige primero un documento completo para recién después leerlo. Compara las representaciones de los fragmentos indexados y conserva los candidatos mejor posicionados. Esto permite combinar, por ejemplo, una explicación de recuperación semántica con otra sobre el uso de contexto externo para generar respuestas.

## Contexto y grounding

Grounding significa apoyar la respuesta en información proporcionada para la tarea. En el RAG básico, el prompt pide utilizar únicamente el contexto recuperado e indicar si no contiene información suficiente. Esta instrucción orienta al modelo, pero no demuestra que vaya a cumplirla siempre. Una respuesta todavía puede omitir detalles, interpretar mal un fragmento o agregar información no sustentada.

La recuperación y la generación tienen responsabilidades diferentes. Si el contexto seleccionado no contiene la respuesta, pedirle al modelo que redacte mejor no incorpora el fragmento que faltaba. Por eso conviene observar el contexto antes de la respuesta. Esa separación permite reconocer si el problema está en qué información llegó al prompt o en cómo fue utilizada por el modelo.

## Fuentes recuperadas

Cada chunk conserva el nombre de su archivo de origen en `Source` y una posición local en `Index`. El contexto puede incluir un encabezado con esos datos antes del contenido. La aplicación también puede mostrar fuente, posición y similitud en la consola. Así se vuelve visible qué fragmentos fueron seleccionados y de dónde proviene el material enviado al modelo.

Mostrar fuentes recuperadas no equivale a verificar citas formales en una respuesta. El modelo puede utilizar una parte del contexto o combinar varias ideas. La procedencia ayuda a revisar el material disponible, pero no garantiza por sí sola que cada afirmación generada esté respaldada. En el ejercicio mantenemos esta distinción y evitamos agregar un sistema de citaciones.

## Cantidad de candidatos

`topK` expresa la cantidad máxima de fragmentos que devolvemos tras ordenar por similitud. Un valor de tres significa hasta tres candidatos disponibles. No significa tres documentos diferentes ni tres pruebas suficientes para responder. Dos o tres resultados pueden proceder del mismo archivo si sus representaciones quedan cerca de la pregunta.

También puede ocurrir que el corpus no tenga información relacionada y aun así existan candidatos para ordenar. Ser el mejor resultado disponible no demuestra que sea relevante. La selección básica sirve para observar ese problema. En este paso del recorrido mostramos los resultados y su contexto sin incorporar una regla adicional para aceptar o rechazar fragmentos por su score.

## Una implementación deliberadamente pequeña

El índice del ejemplo vive en RAM y se vuelve a construir al ejecutar el programa. La búsqueda compara la consulta con todos los vectores almacenados. El chunking usa caracteres y un overlap fijo para mantener visible el mecanismo. Estas decisiones permiten seguir la relación entre documento, fragmento, representación, recuperación, contexto y respuesta sin depender de infraestructura externa.

Cuando incorporamos varios documentos, esas piezas conocidas siguen cumpliendo la misma función. Cambia el alcance del conocimiento disponible: la recuperación considera fragmentos de todas las fuentes. Comprender ese cambio antes de agregar otras capacidades permite explicar qué encontró el sistema, qué material envió al modelo y qué limitaciones todavía conserva el resultado.
