# Lab 16 - MCP

## Objetivo

Comprender Model Context Protocol desde una necesidad conocida: exponer y consumir capacidades mediante un protocolo estándar, después de haber trabajado tools y agentes.

## Estado

⏳ Pendiente de implementación. Este README describe el alcance previsto; todavía no hay código ejecutable.

## El problema que aparece

Hasta aquí las herramientas se integran en nuestra aplicación. Queremos estudiar cómo compartir y consumir esas capacidades mediante un protocolo estándar.

```text
aplicación → cliente MCP → servidor MCP → capacidades
```

MCP llega cuando ya podemos explicar qué es una tool y para qué la utiliza la aplicación. El protocolo tiene una responsabilidad diferente de la lógica de un agente o de un workflow.

## Alcance por diseñar

Consumir un servidor MCP y crear un servidor mínimo son posibilidades para el ejercicio. No se compromete todavía una estructura de sublabs ni una numeración 16a/16b; se definirá al diseñar la implementación.

## Siguiente laboratorio

[Lab 17 - Proyecto final](../lab-17-final-project/README.md), pendiente: decidir qué capacidades necesita una solución e integrarlas conscientemente.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
