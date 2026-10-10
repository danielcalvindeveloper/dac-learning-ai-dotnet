# Enfoque pedagógico y evolución del roadmap

## Contexto

El roadmap inicial buscaba recorrer LLM, RAG, Tools, Agents y MCP. La experiencia de construir los primeros laboratorios mostró el valor de separar algunas de esas capacidades en pasos más pequeños. El recorrido madura a partir de esa experiencia y conserva los Labs 00–17 como camino principal.

## Qué aprendimos construyendo los primeros labs

- En Lab 02 observamos el historial explícito; en Lab 06 organizamos memoria por sesión y una ventana de mensajes.
- En Lab 09a vemos cada etapa del pipeline RAG en `Program.cs`; en Lab 09b encapsulamos el mismo mecanismo en `RagService`.
- En Lab 11a observamos el ciclo explícito de una Tool; en Lab 11b mantenemos el ejercicio y delegamos su coordinación a `FunctionInvokingChatClient`.
- En Lab 12a ofrecemos varias capacidades; 12b observa decisión y errores; 12c conecta una Tool con HTTP. Los tres están implementados y no son agentes.
- En Lab 13 observamos un agente de soporte: objetivo, estado, decisiones, acciones y observaciones dentro de un loop explícito con límite.
- Hasta Lab 06 `AI_MODEL` era suficiente. `AI_EMBEDDING_MODEL` aparece en Lab 07, cuando necesitamos una capacidad distinta.

Estos ejemplos comparten una decisión: introducir una abstracción cuando el alumno ya conoce el problema que resuelve.

## Decisión

```text
comprender
    ↓
experimentar
    ↓
observar limitaciones
    ↓
abstraer cuando exista una razón
    ↓
integrar
```

Primero entender, después abstraer y finalmente integrar. Por ejemplo, observar que `topK` devuelve los mejores fragmentos disponibles permite plantear una pregunta concreta: ¿qué hacemos si ninguno es suficientemente relevante?

## Dos niveles complementarios

**Nivel 1 — comprender:** `labs/` mantiene implementaciones explícitas, pequeñas y observables para experimentar mecanismos y reconocer sus límites.

**Nivel 2 — aplicar:** [`reference/` — Implementaciones de referencia](../reference/README.md) reutiliza esos conceptos con herramientas e infraestructura más cercanas a escenarios reales.

Una implementación pedagógica explícita no es una mala implementación. Tiene un objetivo diferente. Una implementación de referencia no reemplaza la comprensión conceptual: la presupone.

```text
comprender el mecanismo
        ↓
ver una implementación aplicada
        ↓
relacionar ambos niveles
```

Una abstracción o framework adquiere mayor valor pedagógico cuando el alumno ya experimentó el problema que resuelve. Esta relación evita tanto incorporar infraestructura sin comprenderla como detener el recorrido únicamente en mecanismos pedagógicos.

Las referencias podrán aparecer en paralelo cuando un bloque tenga suficiente madurez. Serán ejecutables y acotadas; realista no significa industrial. La regla sigue siendo **primero el problema, después la herramienta**.

## Consecuencia en RAG

El bloque principal llega hasta **Lab 10 - Retrieval y calidad**. La secuencia reúne embeddings, carga de documentos, chunking, RAG explícito, encapsulación en `RagService`, múltiples documentos, retrieval, relevancia y fuentes.

Lab 10a pasa de un documento a un corpus. Lab 10b trabaja un umbral experimental de similitud, la falta de contexto suficiente y las fuentes recuperadas. Ambos están implementados. `topK` limita la cantidad de resultados; no garantiza relevancia. Un umbral necesita calibración y evaluación con preguntas concretas, y no existe un valor universal.

El bloque implementado ya ofrece una base práctica de RAG. [`reference/rag-vector-store/`](../reference/rag-vector-store/README.md) aplica esa base con Microsoft.Extensions.AI, Microsoft.Extensions.VectorData y Qdrant persistente. Las técnicas de mayor complejidad quedan como áreas de profundización.

## Consecuencia en Tools y Agents

```text
Lab 11a: Primer Tool explícito
    ↓
Lab 11b: Invocación automática de Tools
    ↓
Lab 12a: Múltiples Tools
    ↓
Lab 12b: Decisión y errores
    ↓
Lab 12c: Tool externa
    ↓
Lab 13: Primer agente
    ↓
Lab 14: Workflow
    ↓
Lab 15: Multi-agent
    ↓
Lab 16: MCP
    ↓
Lab 17: Proyecto final
```

Una tool expone una operación que ejecuta código .NET. Function calling permite que el modelo solicite esa operación con argumentos; la aplicación ejecuta el código y devuelve el resultado. Tener varias tools disponibles no convierte por sí solo una aplicación en un agente.

Lab 11 se divide en dos experiencias para comprender, experimentar, observar complejidad e introducir una abstracción. 11a muestra deliberadamente el protocolo y preserva explícitamente la representación nativa cuando reconoce el tipo del SDK utilizado, sin preguntar qué proveedor está configurado. 11b utiliza `UseFunctionInvocation()` para encapsular la coordinación ya comprendida. La función sigue ejecutándose dentro de la aplicación.

La prueba inicial de 11b mostró que automatizar el ciclo no corregía por sí solo la pérdida de metadata en el adaptador. La conservación del mensaje nativo quedó en el cliente base, fuera del ejercicio. Gemini y `thought_signature` fueron el caso real que permitió descubrir el problema. El principio aprendido es trabajar contra el modelo común y conservar información nativa cuando el protocolo la necesita. La implementación concreta pertenece al adaptador del SDK OpenAI; otro SDK puede requerir otro tratamiento. Este contraste ayuda a distinguir la responsabilidad del cliente de invocación de la compatibilidad del adaptador. Ambos sublabs están implementados. Lab 12 continúa con tres experiencias también implementadas: selección entre capacidades, decisión de responder sin Tool y errores, e integración de una API HTTP.

12b comprueba que una función debe defender sus invariantes y que `IncludeDetailedErrors` permite devolver al modelo el mensaje de una excepción. 12c distingue una API de la Tool que la encapsula; consultar datos publicados no garantiza que describan el presente. M6 queda implementado. Lab 13 también está implementado y completa M7: coordina pasos sucesivos hacia un objetivo con estado y un máximo de cinco iteraciones.

El primer agente incorpora un objetivo, herramientas, estado/contexto y decisiones iterativas con límites. `UseFunctionInvocation()` conserva el intercambio de funciones; `FunctionInvoker` y `FunctionInvocationContext.Terminate` permiten devolver el control al `while` después de la acción. Function Calling puede estar abstraído; el Agent Loop debe quedar visible. Los escenarios de incidente conocido, error conocido y error desconocido permiten observar caminos distintos, sin routing fijo por reporte. Las decisiones siguen dependiendo del modelo y finalizar no prueba que el diagnóstico sea correcto.

Lab 14 continúa pendiente y prepara la pregunta: ¿todas las decisiones del proceso deberían quedar en manos del agente? Después estudiamos workflows: procesos cuyos pasos y bifurcaciones controla la aplicación. Un workflow puede tener una estructura determinista aunque una llamada a un modelo produzca respuestas variables.

Multi-agent llega cuando ya podemos evaluar si conviene repartir responsabilidades entre pocos agentes. Más agentes no implica una mejor solución. MCP aparece después, cuando conocemos la necesidad de exponer y consumir capacidades mediante un protocolo estándar.

El proyecto final ejercita la elección: RAG, una tool, un workflow, un agente o MCP se incorporan según el problema. También debe poder justificarse que una capacidad no hace falta.

## Estrategia para frameworks en reference

`Microsoft.Extensions.AI` conserva las abstracciones base para integrar modelos. Microsoft.Extensions.VectorData aporta contratos de almacenamiento y búsqueda vectorial cuando el caso necesita esa infraestructura.

Microsoft Agent Framework es el framework objetivo para nuevos escenarios agénticos, sesiones, workflows y orquestación después de comprender el Agent Loop. `reference/agent-framework/` retomará la familia de problemas de incidentes de Lab 13 para relacionar el ciclo explícito con lo que delega el framework. Lab 13 mantiene objetivo, estado, decisión, acción, observación, iteración y condición de finalización visibles, sin migrarlo a MAF.

```text
concepto explícito
        ↓
Microsoft.Extensions.AI: abstracciones base
        ↓
infraestructura especializada, cuando haga falta
        ↓
Microsoft Agent Framework, cuando el problema sea agéntico
```

Esta progresión orienta decisiones; no obliga a que cada aplicación use todas las capas. MAF complementa Microsoft.Extensions.AI. Para Labs 14–15 se considerará como framework principal de workflows, delegación y coordinación, manteniendo el concepto visible y eligiendo APIs al implementar. La [estrategia y sus fuentes oficiales](../reference/README.md#estrategia-de-librerías-y-frameworks) documentan la elección por problema.

La primera referencia de RAG con Qdrant ya es ejecutable y está enlazada desde Labs 07–10. La siguiente referencia, el agente con Microsoft Agent Framework, continúa planificada. Cada implementación debe enlazar sus labs previos, justificar dependencias y explicar preparación, configuración, ejecución, tests, diferencias y limitaciones.

## Por qué el recorrido es más gradual

Cada laboratorio plantea una pregunta pequeña y concreta. Hay más pasos para experimentar y reconocer limitaciones antes de combinar capacidades. El alumno puede identificar qué problema apareció, qué solución introducimos y por qué existe.

Los contrastes entre 09a y 09b y entre 11a y 11b permiten valorar qué aporta encapsular una responsabilidad después de comprenderla. 11a muestra el mecanismo; 11b utiliza la abstracción adecuada del framework para coordinarlo. Los frameworks se incorporan cuando ayudan a resolver una necesidad que ya podamos explicar.

## Extensiones futuras

Las posibles extensiones no tienen numeración ni estructura comprometida. Una **Extensión - RAG avanzado** podría profundizar en:

- filtros por metadata y técnicas de recuperación sobre bases vectoriales reales;
- hybrid search y BM25;
- re-ranking, query rewriting y multi-query;
- chunking semántico;
- evaluación de retrieval y de respuestas;
- observabilidad y citaciones avanzadas.

La primera aplicación de infraestructura vectorial ya existe en `reference/` y la referencia con Microsoft Agent Framework continúa planificada, sin esperar a una extensión avanzada. Estas extensiones podrán profundizar después en técnicas más complejas.

También podrán estudiarse Agents avanzado, MCP avanzado, evaluación y observabilidad u otras áreas que surjan. Una vez adquirida la base, estas técnicas pueden explorarse con más contexto y preguntas mejor definidas. La lista permanece abierta.

## Principio rector

Cada nueva herramienta debe aparecer cuando el alumno ya comprende el problema que resuelve.

[Roadmap y estados de implementación](../ROADMAP.md) · [Recorrido por laboratorios](03-roadmap-labs.md).
