# Lab 15b - Coordinación con Microsoft Agent Framework

Este sublab crea tres `ChatClientAgent` especialistas y los conecta como executors en un `WorkflowBuilder`. `Program.cs` conserva los prompts y contratos; MAF ejecuta el grafo `TriageAgent → DiagnosisAgent → ResolutionAgent`.

Cada executor llama a su agente con una entrada nueva y estructurada, no reenvía todo el historial. MAF coordina el flujo; los datos locales y la regla para registrar un incidente siguen bajo control de la aplicación.

Desde la raíz del repositorio:

```powershell
dotnet run --project .\labs\lab-15-multi-agent\lab-15b-multi-agent-maf\Lab15b.MultiAgentMaf.csproj
```

Requiere .NET 10, Microsoft Agent Framework 1.26.0, conectividad con un modelo compatible con structured outputs y la configuración del `.env` raíz. La guía del [Lab 15](../README.md) muestra la trazabilidad entre el grafo y el código.
