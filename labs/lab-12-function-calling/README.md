# Lab 12 - Function Calling

## Objetivo

Trabajar Function Calling con múltiples Tools: selección, argumentos, ejecución, errores y casos donde el modelo responde sin usar ninguna. El ciclo explícito y su automatización con una única Tool ya se estudian en Lab 11.

## Estado

⏳ Pendiente de implementación. Este README describe el alcance previsto; todavía no hay código ejecutable.

## Concepto

```text
LLM
 ├── Tool A
 ├── Tool B
 └── Tool C
```

[Lab 11a](../lab-11-tools/lab-11a-tool-explicita/README.md) muestra el protocolo explícito y [Lab 11b](../lab-11-tools/lab-11b-tool-invocation/README.md) introduce `UseFunctionInvocation()` con el mismo ejercicio. Aquí utilizaremos esa base para observar selección, argumentos y resultados cuando hay más de una herramienta disponible. Introducir la invocación automática ya no es el objetivo principal de Lab 12.

Entre los casos didácticos habrá al menos una tool cuya implementación invoque una capacidad externa, por ejemplo una API HTTP. La tool encapsulará esa llamada para observar argumentos, ejecución, resultados y errores en un ejemplo acotado.

## Distinciones que observaremos

```text
API externa ≠ Tool
Tool ≠ Agent
Tool ≠ MCP
```

Una tool puede encapsular una API. Un agente puede utilizar tools. MCP puede exponer tools y permitir descubrirlas mediante un protocolo estándar. Son responsabilidades distintas; Lab 12 se concentra en comprender y ejecutar múltiples tools.

## Alcance previsto

- Describir herramientas con responsabilidades distintas.
- Utilizar el ciclo de invocación automática ya comprendido en 11b.
- Observar los argumentos recibidos y relacionarlos con cada operación.
- Observar cuándo usar una y cuándo no usar ninguna.
- Explorar una pregunta que pueda requerir más de una operación.
- Incluir una tool que encapsule una llamada a una API externa, por ejemplo HTTP.
- Contrastar una Tool local con otra que encapsule una capacidad externa.
- Manejar errores básicos de ejecución y presentar resultados.

## Fuera de alcance

Agentes, multi-agent e implementación de MCP. La llamada externa será un caso didáctico pequeño; el foco sigue siendo selección, argumentos, ejecución y errores. Tener varias tools, o ejecutar más de una operación, no convierte por sí solo el mecanismo en un agente.

## Siguiente laboratorio

[Lab 13 - Primer agente](../lab-13-agents/README.md), pendiente: objetivo, estado/contexto y decisiones iterativas con límites.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
