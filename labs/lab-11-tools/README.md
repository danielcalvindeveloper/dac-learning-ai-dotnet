# Lab 11 - Primer Tool

## Objetivo

Comprender cómo el modelo solicita una operación y cómo la aplicación ejecuta código .NET para resolverla. El ejemplo previsto utilizará una única tool pequeña.

## Estado

⏳ Pendiente de implementación. Este README describe el alcance previsto; todavía no hay código ejecutable.

## Concepto

```text
pregunta → modelo → solicitud de tool + argumentos
                          ↓
                      código .NET
                          ↓
                       resultado → modelo → respuesta
```

Una tool expone una operación. Function calling es el mecanismo por el que el modelo solicita ejecutarla con argumentos. La aplicación ejecuta la función y devuelve el resultado; el modelo no ejecuta código mágicamente.

## Alcance previsto

- Describir la finalidad de una tool.
- Observar los argumentos solicitados.
- Ejecutar la función y devolver su resultado.
- Obtener la respuesta final con ese resultado.

## Fuera de alcance

Múltiples tools, agentes y multi-agent. Una solicitud de herramienta no se presenta como un agente.

## Siguiente laboratorio

[Lab 12 - Múltiples Tools](../lab-12-function-calling/README.md), pendiente.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
