# Lab 12 - Múltiples Tools

## Objetivo

Pasar de una tool a varias operaciones disponibles y observar cómo el modelo solicita la que corresponde a la pregunta, o responde sin usar ninguna.

## Estado

⏳ Pendiente de implementación. Este README describe el alcance previsto; todavía no hay código ejecutable.

## Concepto

```text
LLM
 ├── Tool A
 ├── Tool B
 └── Tool C
```

Function calling ya habrá sido introducido en Lab 11. Aquí observaremos selección, argumentos y resultados cuando hay más de una herramienta disponible.

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
- Observar cuándo usar una y cuándo no usar ninguna.
- Explorar una pregunta que pueda requerir más de una operación.
- Incluir una tool que encapsule una llamada a una API externa, por ejemplo HTTP.
- Manejar errores básicos de ejecución y presentar resultados.

## Fuera de alcance

Agentes, multi-agent e implementación de MCP. La llamada externa será un caso didáctico pequeño; el foco sigue siendo selección, argumentos, ejecución y errores. Tener varias tools, o ejecutar más de una operación, no convierte por sí solo el mecanismo en un agente.

## Siguiente laboratorio

[Lab 13 - Primer agente](../lab-13-agents/README.md), pendiente: objetivo, estado/contexto y decisiones iterativas con límites.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
