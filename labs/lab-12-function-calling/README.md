# Lab 12 - Function Calling

## Objetivo

Observar qué cambia cuando el modelo dispone de varias capacidades: seleccionar una Tool, generar argumentos, responder sin usar ninguna, recibir un error e integrar una API HTTP.

Partimos de [Lab 11b](../lab-11-tools/lab-11b-tool-invocation/README.md). El ciclo ya se automatiza con `UseFunctionInvocation()`; ahora estudiamos las decisiones y los resultados de esas invocaciones.

**El modelo solicita. La aplicación ejecuta.**

## Estado

✅ Implementado mediante tres sublabs independientes. Los tres completaron restore, build y ejecución real con `gemini-3.5-flash-lite` y .NET 10. M6 queda ✅ Implementado. Lab 13 continúa ⏳ Pendiente.

| Sublaboratorio | Pregunta principal | Estado |
|---|---|---|
| [12a - Múltiples Tools](lab-12a-multiples-tools/README.md) | ¿Qué capacidad solicita el modelo según la pregunta? | ✅ Implementado |
| [12b - Decisión y errores](lab-12b-decision-y-errores/README.md) | ¿Cuándo no hace falta una Tool y qué pasa si falla? | ✅ Implementado |
| [12c - Tool externa](lab-12c-tool-externa/README.md) | ¿Cómo puede una Tool consultar una API HTTP? | ✅ Implementado |

## Progresión

```text
una Tool
    ↓
varias Tools
    ↓
decidir
    ↓
fallar de manera controlada
    ↓
integrar una capacidad externa
```

Primero entender, después abstraer y finalmente integrar. Cada sublab plantea una pregunta diferente sin volver a implementar manualmente Function Call, Function Result ni la continuación.

## Estructura

```text
lab-12-function-calling/
├── README.md
├── lab-12a-multiples-tools/
├── lab-12b-decision-y-errores/
└── lab-12c-tool-externa/
```

Cada sublab contiene su propio proyecto, `Program.cs`, `LabConfiguration.cs`, `ChatClientFactory.cs` y la implementación concreta de sus Tools. Sus README incluyen los diagramas de secuencia y las instrucciones de ejecución. La explicación cabe allí; no agregamos documentos complementarios por duplicar contenido.

La configuración y la factory son copias del patrón de 11b. La preservación de `RawRepresentation` queda en el cliente base y actúa sobre el tipo nativo del SDK OpenAI, sin condiciones por proveedor. No creamos una biblioteca compartida ni un framework de Tools.

## Configuración

Los tres utilizan el `.env` de la raíz:

```env
AI_PROVIDER=
AI_MODEL=
AI_API_KEY=
AI_URL=
```

Proveedor, modelo y credencial son obligatorios. `AI_URL` es opcional para un endpoint compatible. `AI_MODEL` debe soportar Function Calling en ese endpoint. No se utiliza `AI_EMBEDDING_MODEL` ni se agregan variables.

En 12a y 12b las Tools son locales, pero las consultas al modelo requieren conexión. En 12c también se accede a una API pública de países sin autenticación adicional.

## Qué observamos

- 12a: el modelo solicitó IVA, descuento y conversión de temperatura; .NET calculó `1210`, `2125` y `86`.
- 12b: respondió una explicación sin Tool. Después solicitó un descuento del `150%`; la función lo rechazó y el modelo explicó el error recibido.
- 12c: `ConsultarPais` llamó a `countries.dev` y obtuvo HTTP 200 para Argentina y Uruguay; un nombre inexistente produjo HTTP 404 y una respuesta comprensible.

Son observaciones de la configuración probada, no decisiones universales que deban repetir todos los modelos. Cada README explica la validación y sus límites.

## Qué aprendemos

Al completar los tres sublabs deberías poder explicar:

1. cómo ofrecer varias capacidades simultáneamente;
2. por qué la selección depende de la definición de la Tool y la pregunta;
3. por qué Tools disponibles no significa Tool obligatoria;
4. por qué la función debe validar sus argumentos;
5. qué recibe el modelo cuando falla una Tool local;
6. cómo una Tool puede encapsular una llamada HTTP;
7. por qué API REST, Tool, Agent y MCP representan responsabilidades distintas.

## Comparación final

| Etapa | Capacidad recorrida |
|---|---|
| Lab 11a | Una Tool y mecanismo explícito |
| Lab 11b | Una Tool e invocación automática |
| Lab 12a | Varias capacidades disponibles simultáneamente |
| Lab 12b | Decisión de no usar Tool y error de ejecución |
| Lab 12c | Tool que encapsula una capacidad externa |

```text
API REST ≠ Tool
Tool ≠ Agent
Tool ≠ MCP
```

Una Tool puede encapsular una API REST. Un Agent puede utilizar Tools. MCP puede exponer capacidades y permitir descubrirlas. Aquí la Tool llama directamente a HTTP; no utilizamos MCP ni implementamos un agente.

## Siguiente laboratorio

[Lab 13 - Primer agente](../lab-13-agents/README.md) sigue pendiente. Hasta aquí el usuario hace una pregunta, el modelo puede solicitar Tools y produce una respuesta.

El siguiente paso estudiará un sistema que reciba un objetivo, evalúe estado, decida pasos sucesivos y continúe hasta cumplirlo o alcanzar un límite. Ese comportamiento todavía no se implementa en Lab 12.

[Lab 11 - Tools](../lab-11-tools/README.md) · [Roadmap](../../ROADMAP.md) · [Enfoque pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
