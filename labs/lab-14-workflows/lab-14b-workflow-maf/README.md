# Lab 14b — Workflow con Microsoft Agent Framework

## Objetivo

Construir con MAF el mismo workflow que en 14a. Cambia cómo se conectan y ejecutan los pasos; se mantienen el dominio, el estado y las reglas.

## Qué mirar

- `Program.cs`: `CreateWorkflow`, executors, `WorkflowBuilder`, edges condicionales y ejecución.
- `../Shared/IncidentWorkflowDomain.cs`: estado y pasos de dominio compartidos con 14a.
- `../data/`: datos propios de Lab 14.
- Paquete `Microsoft.Agents.AI.Workflows` **1.26.0** en .NET 10; NuGet lista esta versión sin etiqueta prerelease.

## Cómo ejecutar

Desde la raíz del repositorio:

```powershell
dotnet run --project .\labs\lab-14-workflows\lab-14b-workflow-maf
```

## Qué observar

- `BindAsExecutor` convierte cada paso compartido en un executor.
- `AddEdge` expresa las transiciones; los predicados expresan las mismas bifurcaciones que los `if` de 14a.
- MAF ejecuta el grafo; la aplicación sigue definiendo sus condiciones.
- La consola muestra los tres recorridos para compararlos con 14a.

## Más información

[Guía pedagógica, diagrama trazable y documentación oficial](../README.md)
