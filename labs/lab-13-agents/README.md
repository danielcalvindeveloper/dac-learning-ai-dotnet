# Lab 13 - Primer agente

## Objetivo

Introducir un agente pequeño y comprensible después de conocer tools y function calling.

## Estado

⏳ Pendiente de implementación. Este README describe el alcance previsto; todavía no hay código ejecutable.

## Concepto

```text
objetivo → modelo + herramientas + estado/contexto
                    ↓
             decisiones iterativas
                    ↓
          resultado o límite de ejecución
```

Tool calling es una capacidad. El agente coordina decisiones y acciones hacia un objetivo, manteniendo el contexto del proceso. Una llamada aislada a una tool no demuestra ese comportamiento.

## Alcance previsto

- Definir un objetivo y herramientas acotadas.
- Conservar estado/contexto entre decisiones.
- Observar el ciclo de decisión y acción.
- Definir condiciones de finalización y límites.

## Fuera de alcance

Multi-agent y orquestaciones complejas. Primero necesitamos comprender y observar un solo agente.

## Siguiente laboratorio

[Lab 14 - Workflow](../lab-14-multi-agent/README.md), pendiente: evaluar un proceso de pasos explícitos como alternativa para problemas conocidos.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
