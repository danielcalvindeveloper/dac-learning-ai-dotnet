# Qué aprendemos en los laboratorios implementados

Recopilación de los aprendizajes de los Labs 00–13, incluidos sus sublaboratorios. Cada título enlaza al README correspondiente.

Se conserva el contenido de «Qué aprendemos»; en Labs 01 y 02 se utiliza «Resultado esperado». Para Lab 00 y Lab 10b se sintetizan los aprendizajes a partir de sus README, que no tienen una sección equivalente.

## [Lab 00 - Introducción a los artefactos de Fundamentos](../labs/lab-00-introduccion/README.md)

Al finalizar deberías poder explicar:

1. cómo enviar una pregunta a un modelo desde una consola .NET y mostrar la respuesta;
2. qué diferencia hay entre modelo, proveedor, cliente concreto e `IChatClient`;
3. qué representan el prompt, `ChatResponse` y `response.Text`;
4. para qué sirven el endpoint y la API key;
5. cómo proveedores diferentes pueden utilizarse mediante `IChatClient`;
6. por qué este ejemplo mínimo es una rampa opcional y la configuración se externaliza desde Lab 01.

## [Lab 01 - Hello LLM](../labs/lab-01-hello-llm/README.md)

Al finalizar deberías poder explicar:

1. qué representa `IChatClient`;
2. qué representa un prompt;
3. cómo se envía un prompt al modelo;
4. qué es `ChatResponse`;
5. dónde se encuentra el texto devuelto;
6. por qué la configuración del proveedor no necesita ocupar el código principal.

## [Lab 02 - Chat History](../labs/lab-02-chat-history/README.md)

Al finalizar deberías poder explicar:

1. por qué dos llamadas independientes no comparten contexto;
2. qué representa `List<ChatMessage>`;
3. qué representa `ChatMessage`;
4. por qué guardamos también las respuestas del asistente;
5. por qué el modelo conoce el nombre en el segundo caso;
6. por qué el historial pertenece a la aplicación y no al modelo.

## [Lab 03 - Services y Dependency Injection](../labs/lab-03-services-di/README.md)

Al finalizar este laboratorio deberías poder explicar:

1. qué diferencia hay entre una interfaz y su implementación;
2. qué significa inyección por constructor;
3. para qué sirve `ServiceCollection`;
4. qué hace `AddSingleton`;
5. qué hace `AddTransient`;
6. por qué `Program.cs` ya no necesita construir `Assistant`;
7. por qué `Assistant` recibe `IChatClient` en lugar de crearlo;
8. cómo una operación como `TraducirAsync()` puede encapsular un prompt.

## [Lab 04 - Prompt Templates](../labs/lab-04-prompt-templates/README.md)

Al finalizar deberías poder explicar:

1. qué es un Prompt Template;
2. qué parte permanece fija;
3. qué partes son variables;
4. por qué `PromptTemplates` tiene una responsabilidad diferente a `Assistant`;
5. por qué `Assistant` sigue siendo el servicio;
6. cómo reutilizar un template con distintos parámetros;
7. por qué `PromptTemplates` no necesita DI en este ejemplo;
8. cómo el Lab 04 evoluciona el diseño del Lab 03 sin reemplazarlo.

## [Lab 05 - Structured Output](../labs/lab-05-structured-output/README.md)

Al finalizar deberías poder explicar:

1. qué significa Structured Output;
2. por qué un tipo C# puede actuar como contrato de salida;
3. qué representa `ChatResponse<T>`;
4. qué hace `GetResponseAsync<T>()`;
5. para qué sirve `TryGetResult`;
6. por qué `Assistant` devuelve objetos y no JSON o strings;
7. por qué `Personas` utiliza un objeto raíz;
8. por qué estos modelos no son servicios y no se registran en DI;
9. cómo Structured Output facilita integrar la respuesta con código de aplicación.

## [Lab 06 - Chat Memory Advanced](../labs/lab-06-chat-memory-advanced/README.md)

Al finalizar deberías poder explicar:

1. por qué una sola lista no alcanza para varias conversaciones;
2. qué representa `sessionId`;
3. para qué existe `IChatMemoryStore`;
4. qué responsabilidad tiene `InMemoryChatMemoryStore`;
5. por qué `Assistant` no conoce el `Dictionary`;
6. por qué el store se registra como Singleton;
7. por qué `Assistant` puede seguir siendo Transient;
8. cómo funciona la ventana de 20 mensajes;
9. por qué esta memoria desaparece al cerrar el proceso;
10. cómo podría reemplazarse el almacenamiento sin modificar `Assistant`.

## [Lab 07 - Embeddings](../labs/lab-07-embeddings/README.md)

Al finalizar deberías poder explicar:

1. qué es un embedding;
2. qué representa el vector;
3. para qué sirve `IEmbeddingGenerator`;
4. por qué dos textos relacionados deberían tener mayor similitud;
5. qué mide la similitud coseno;
6. por qué un modelo de embeddings es conceptualmente diferente de un modelo de chat;
7. por qué comparar representaciones semánticas será útil posteriormente para recuperar contenido relevante en RAG.

## [Lab 08 - Document Loading](../labs/lab-08-document-loading/README.md)

Al finalizar deberías poder explicar:

1. qué significa cargar un documento;
2. por qué convertimos el archivo a texto;
3. qué es un chunk;
4. qué representa `chunkSize`;
5. qué representa `overlap`;
6. para qué sirve `Source`;
7. para qué sirve `Index`;
8. por qué todavía no estamos haciendo RAG;
9. cómo estos chunks podrán utilizarse posteriormente.

## [Lab 09 - RAG básico](../labs/lab-09-rag-basico/README.md)

### [Lab 09a - RAG explícito](../labs/lab-09-rag-basico/lab-09a-rag-explicito/README.md)

Al finalizar deberías poder explicar:

1. qué significa RAG;
2. la diferencia entre indexación y consulta;
3. por qué generamos embeddings de los chunks;
4. por qué generamos un embedding de la pregunta;
5. qué hace el vector store;
6. qué significa topK;
7. cómo construimos el contexto;
8. por qué el modelo recibe contexto recuperado;
9. la diferencia entre `AI_MODEL` y `AI_EMBEDDING_MODEL`;
10. por qué primero observamos el pipeline sin encapsularlo en un servicio.

### [Lab 09b - RAG con RagService](../labs/lab-09-rag-basico/lab-09b-rag-service/README.md)

Al finalizar deberías poder explicar:

1. qué responsabilidad encapsula `RagService`;
2. por qué aparece `IRagService`;
3. la diferencia entre mecanismo RAG y servicio RAG;
4. por qué el servicio recibe sus dependencias;
5. por qué no crea `IChatClient`;
6. por qué no crea `IEmbeddingGenerator`;
7. por qué `InMemoryVectorStore` conserva el índice;
8. para qué sirve `RagResponse`;
9. por qué `Program.cs` queda más simple;
10. por qué 09a sigue siendo importante aunque 09b reduzca la orquestación del programa.

## [Lab 10 - Retrieval y calidad](../labs/lab-10-retrieval-calidad/README.md)

### [Lab 10a - Múltiples documentos](../labs/lab-10-retrieval-calidad/lab-10a-multiples-documentos/README.md)

1. Qué cambia al pasar de un documento a un corpus.
2. Cómo indexar varios archivos sin separar el store por fuente.
3. Por qué un chunk conserva `Source` e `Index`, aunque se repitan posiciones entre archivos.
4. Por qué la consulta genera un solo embedding y busca sobre todo el índice.
5. Cómo varios documentos pueden aportar contexto a una respuesta.
6. Por qué `topK` no garantiza relevancia ni diversidad de fuentes.

### [Lab 10b - Relevancia, threshold y fuentes](../labs/lab-10-retrieval-calidad/lab-10b-relevancia-threshold-fuentes/README.md)

Al finalizar deberías poder explicar:

1. por qué `topK` devuelve los mejores candidatos disponibles sin garantizar que sean relevantes;
2. qué diferencia hay entre ordenar por similitud y filtrar mediante `minimumSimilarity`;
3. por qué el threshold es experimental y debe calibrarse según el modelo, corpus, preguntas, chunking y dominio;
4. por qué un score no es un porcentaje de relevancia ni una probabilidad de respuesta correcta;
5. cómo construir el contexto sólo con los candidatos aceptados;
6. por qué evitamos la llamada generativa cuando no queda contexto suficiente, aunque todavía generamos el embedding de la consulta;
7. qué diferencia hay entre las fuentes del contexto enviado y las citas generadas por el LLM;
8. por qué aceptar un chunk no garantiza que cubra la pregunta ni que la respuesta final esté respaldada.

## [Lab 11 - Tools](../labs/lab-11-tools/README.md)

Al completar las dos experiencias deberías poder explicar qué son Tool, Function Call y Function Result; dónde se ejecuta el método C#; qué complejidades aparecen al coordinar el ciclo y qué responsabilidad encapsula `FunctionInvokingChatClient`.

### [Lab 11a - Primer Tool explícito](../labs/lab-11-tools/lab-11a-tool-explicita/README.md)

Al finalizar deberías poder explicar:

1. qué capacidad expone una Tool y cómo se relaciona con una función C#;
2. qué recibe el modelo mediante `ChatOptions.Tools`;
3. qué representan `FunctionCallContent` y sus argumentos;
4. por qué el modelo solicita y la aplicación ejecuta;
5. qué hace `AIFunction.InvokeAsync`;
6. para qué sirven `FunctionResultContent`, el rol `Tool` y `CallId`;
7. por qué reenviamos la solicitud junto con el resultado;
8. cómo el modelo utiliza ese resultado para redactar la respuesta final;
9. por qué una Tool no convierte este ejercicio en un agente;
10. qué diferencia hay entre el modelo común y `RawRepresentation`, y por qué preservamos el mensaje nativo al continuar.

### [Lab 11b - Invocación automática de Tools](../labs/lab-11-tools/lab-11b-tool-invocation/README.md)

1. Qué responsabilidad de 11a encapsula `FunctionInvokingChatClient`.
2. Qué agregan `ChatClientBuilder` y `UseFunctionInvocation()`.
3. Por qué una sola llamada de aplicación puede implicar varios intercambios con el modelo.
4. Por qué la Tool continúa ejecutándose en .NET.
5. Qué sigue decidiendo y generando el modelo.
6. Por qué abstraer el ciclo no garantiza compatibilidad absoluta del adaptador.

## [Lab 12 - Function Calling](../labs/lab-12-function-calling/README.md)

Al completar los tres sublabs deberías poder explicar:

1. cómo ofrecer varias capacidades simultáneamente;
2. por qué la selección depende de la definición de la Tool y la pregunta;
3. por qué Tools disponibles no significa Tool obligatoria;
4. por qué la función debe validar sus argumentos;
5. qué recibe el modelo cuando falla una Tool local;
6. cómo una Tool puede encapsular una llamada HTTP;
7. por qué API REST, Tool, Agent y MCP representan responsabilidades distintas.

### [Lab 12a - Múltiples Tools](../labs/lab-12-function-calling/lab-12a-multiples-tools/README.md)

1. Cómo registrar tres Tools al mismo tiempo.
2. Qué papel tienen nombre, descripción y parámetros en la selección.
3. Por qué la aplicación ofrece capacidades y el modelo solicita cuál utilizar.
4. Cómo observar argumentos y ejecución sin instrumentación adicional.
5. Por qué cada consulta puede ser independiente aunque reutilice el cliente.
6. Por qué una decisión observada no es una garantía para todos los modelos.

### [Lab 12b - Decisión y errores](../labs/lab-12-function-calling/lab-12b-decision-y-errores/README.md)

1. Por qué disponer de Tools no obliga a utilizarlas.
2. Cómo distinguir una respuesta directa de una ejecución.
3. Por qué una Tool debe validar sus invariantes.
4. Qué sucede con una excepción de Tool en la versión utilizada.
5. Qué cambia al activar `IncludeDetailedErrors`.
6. Por qué un error local no equivale a un error HTTP del proveedor LLM.
7. Por qué la decisión del modelo y su explicación pueden variar.

### [Lab 12c - Tool externa](../labs/lab-12-function-calling/lab-12c-tool-externa/README.md)

1. Por qué una Tool representa una capacidad y puede encapsular una API.
2. Qué diferencia hay entre la API externa y la Tool ofrecida al modelo.
3. Cómo usar `HttpClient` y deserializar sólo los campos necesarios.
4. Cómo convertir errores HTTP previstos en resultados comprensibles.
5. Por qué consultar un servicio no garantiza que sus datos sean de hoy.
6. Por qué una Tool puede llamar directamente a HTTP sin MCP.
7. Por qué todo este código sigue ejecutándose en la aplicación, no dentro del LLM.

## [Lab 13 - Primer agente](../labs/lab-13-primer-agente/README.md)

Al finalizar deberías poder explicar:

1. qué diferencia hay entre Tool y Agent;
2. qué diferencia hay entre Function Calling y Agent;
3. qué es un objetivo;
4. qué contiene el estado de ejecución;
5. qué representa una observación;
6. qué cuenta como una iteración;
7. cómo funciona el Agent Loop;
8. por qué el agente puede tomar caminos distintos;
9. por qué necesita un límite de iteraciones;
10. por qué un agente no es simplemente un conjunto de Tools.
