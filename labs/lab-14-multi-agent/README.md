# Lab 14 - Workflow

## Objetivo

Modelar un proceso conocido con pasos y bifurcaciones controlados por la aplicación. Aplicar KISS al decidir si un workflow basta para resolver el problema.

## Estado

⏳ Pendiente de implementación. Este README describe el alcance previsto; todavía no hay código ejecutable.

## Concepto

```text
Paso A → Paso B → decisión
                   ├── C
                   └── D
```

Un workflow sigue una estructura explícita; un agente toma decisiones iterativas hacia un objetivo. No son sinónimos. La estructura de un workflow puede ser determinista aunque una llamada a un modelo produzca resultados variables.

## Alcance previsto

- Definir pasos y pasar resultados entre ellos.
- Incorporar una decisión con ramas visibles.
- Observar qué controla la aplicación.
- Justificar cuándo el proceso no necesita un agente.

## Orientación tecnológica futura

Microsoft Agent Framework será el framework principal a considerar para implementar workflows cuando aporte valor al caso. El foco seguirá en comprender pasos, resultados y bifurcaciones; el proceso no necesita incorporar un agente sólo por usar un framework. Las APIs y el diseño concreto se decidirán al implementar. Microsoft.Extensions.AI conserva su papel de base para integrar modelos.

## Fuera de alcance

Multi-agent, motores de orquestación complejos y agentes incorporados sin una necesidad concreta.

## Siguiente laboratorio

[Lab 15 - Multi-agent](../lab-15-workflows/README.md), pendiente.

La ruta de esta carpeta pertenece al esqueleto documental previo y se adecuará al alcance al implementar el laboratorio.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
