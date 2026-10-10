# Microsoft.Extensions.AI en una aplicación .NET

## Un vocabulario común

Microsoft.Extensions.AI ofrece abstracciones para integrar capacidades de inteligencia artificial en aplicaciones .NET. Permite expresar operaciones como conversar con un modelo o generar representaciones vectoriales mediante contratos comunes. La aplicación puede concentrarse en los datos que envía y en los resultados que recibe, mientras un adaptador conecta ese contrato con el cliente concreto del proveedor elegido.

El punto de partida es una necesidad de la aplicación. Por ejemplo, queremos enviar una pregunta y mostrar una respuesta. Para comprender ese flujo conviene distinguir la configuración, la creación del cliente y la llamada que realiza el ejercicio. Son responsabilidades diferentes, aunque un primer programa pueda reunirlas en pocas líneas. Separarlas cuando crece el ejemplo facilita reconocer qué parte enseña el concepto principal.

## Papel de IChatClient

`IChatClient` representa la capacidad de conversación y generación. El código consumidor trabaja con esta interfaz para enviar mensajes y obtener una respuesta. No necesita construir solicitudes HTTP específicas del proveedor en cada interacción. Un cliente adaptado implementa el contrato y se ocupa de traducir las operaciones hacia la API correspondiente.

En un ejemplo simple, el programa prepara un prompt, llama a `GetResponseAsync` y obtiene un `ChatResponse`. La propiedad `Text` permite mostrar el texto de la respuesta. El prompt sigue siendo importante: la abstracción organiza la comunicación, pero no decide por nosotros qué preguntar ni qué instrucciones dar al modelo. La calidad del resultado depende también de la información enviada.

## Mensajes y conversación

Una conversación puede expresarse mediante mensajes con roles y contenido. `ChatMessage` permite representar esos mensajes. La aplicación decide cuáles conserva y cuáles vuelve a enviar en cada turno. Un historial explícito ayuda a observar cómo una pregunta posterior depende del contexto anterior, sin atribuir al cliente una memoria automática que el programa no ha implementado.

El historial pertenece a la aplicación. Si se requieren conversaciones independientes, hay que asociar los mensajes con cada sesión y elegir una política de conservación. Una ventana de mensajes puede limitar el contexto enviado. Estas decisiones no se resuelven simplemente por utilizar una interfaz de chat: son parte del comportamiento que queremos construir sobre esa capacidad básica.

## Respuestas y tipos

`ChatResponse` contiene el resultado de una operación de chat. En los ejercicios más pequeños basta con leer su texto. Cuando necesitamos datos estructurados podemos definir tipos C# que expresen el resultado esperado. Esto permite que el resto del programa trabaje con propiedades y objetos, aunque todavía deba manejar respuestas que no puedan materializarse correctamente.

Una respuesta estructurada no reemplaza al modelo generativo. Cambia la forma en que solicitamos y consumimos el resultado. Pedir una persona con nombre y edad tiene un propósito distinto de pedir un párrafo explicativo. El tipo elegido debe responder a una necesidad concreta del programa, evitando estructuras adicionales que no aporten claridad al ejercicio.

## Capacidades distintas

La biblioteca también ofrece `IEmbeddingGenerator<string, Embedding<float>>` para transformar textos en embeddings. El contrato recibe entradas de texto y devuelve representaciones cuyos vectores contienen valores numéricos. Esa operación no produce una respuesta conversacional. La aplicación elige la abstracción de acuerdo con la capacidad que necesita en cada etapa.

Un modelo configurado para chat no debe asumirse válido para generar embeddings. La compatibilidad depende del modelo y del endpoint utilizado. Por eso una aplicación puede conservar un modelo generativo y otro de embeddings, aun cuando ambos pertenezcan al mismo proveedor. Mantener nombres de configuración distintos hace visible esa diferencia sin multiplicar credenciales innecesariamente.

## Adaptadores y proveedor

Un adaptador permite utilizar un SDK concreto mediante las abstracciones comunes. En los laboratorios, las factories reciben configuración validada, construyen los clientes y devuelven los contratos que consumirá el programa. El flujo principal puede entonces mostrar llamadas de chat o generación de embeddings sin repetir detalles de credenciales y construcción del endpoint.

Este desacoplamiento tiene un límite práctico: usar un contrato común no hace idénticos todos los proveedores. Hay diferencias de modelos, capacidades, cuotas, latencia y disponibilidad. Un endpoint compatible con operaciones de chat puede no ofrecer embeddings. Es necesario elegir explícitamente una combinación que soporte las operaciones del ejercicio y manejar los errores que pueda devolver el servicio.

## Configuración explícita

La configuración del repositorio utiliza `AI_PROVIDER`, `AI_MODEL`, `AI_EMBEDDING_MODEL`, `AI_API_KEY` y `AI_URL`. El modelo de chat se utiliza para generación; el de embeddings, para vectores. La URL alternativa es opcional. Las credenciales se mantienen fuera del código y el programa requiere las capacidades que efectivamente utiliza, sin inventar valores por proveedor.

Separar la carga de configuración del uso del cliente permite leer el ejercicio con más facilidad. La factory no debe conocer los documentos, la pregunta ni el contexto recuperado. El código de aplicación conserva esas decisiones. Así, cambiar la configuración activa no requiere modificar las instrucciones del ejemplo ni introducir condiciones de proveedor en cada llamada.

## Una pieza dentro de la aplicación

Microsoft.Extensions.AI proporciona contratos de acceso a capacidades de modelos. La aplicación sigue siendo responsable de cargar archivos, organizar mensajes, construir prompts, mostrar resultados y decidir qué información utilizar. Las abstracciones ayudan a ordenar estas responsabilidades, pero el comportamiento visible surge de cómo las combinamos para resolver un problema concreto.

El criterio práctico es reconocer primero la operación que necesitamos y después elegir el contrato adecuado. Una llamada pequeña y observable permite aprender más que una estructura que oculta el flujo detrás de muchas capas. Las interfaces comunes tienen valor cuando hacen más clara la relación entre el programa y sus dependencias, manteniendo el concepto del ejercicio a la vista.
