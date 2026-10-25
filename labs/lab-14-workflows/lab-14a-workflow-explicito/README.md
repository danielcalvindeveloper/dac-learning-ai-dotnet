# Lab 14a — Workflow explícito

## Objetivo

Implementar el flujo conocido con pasos y condiciones de C#. La aplicación controla el siguiente paso; no intervienen un agente ni un motor de workflows.

## Qué mirar

- `Program.cs`: estado inicial, secuencia, bifurcaciones `if` y finalización.
- `Shared/IncidentWorkflowDomain.cs`: `IncidentWorkflowState` y pasos de dominio que actualizan ese estado.
- `data/`: estado local de servicios y soluciones.

## Ejecución

Desde la raíz del repositorio, ejecuta:

```powershell
dotnet run --project .\labs\lab-14-workflows\lab-14a-workflow-explicito
```

## Qué observar

- El estado se crea al principio y viaja por cada paso.
- La aplicación decide el siguiente paso en los `if`.
- Cada escenario muestra su ruta en consola y termina construyendo una respuesta.

## Más información

[Guía pedagógica y comparación 14a/14b](../README.md)
