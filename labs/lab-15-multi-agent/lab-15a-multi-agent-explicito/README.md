# Lab 15a - Coordinación explícita

La aplicación coordina tres agentes especialistas desde [Program.cs](./Program.cs): `RunTriageAgentAsync`, `RunDiagnosisAgentAsync` y `RunResolutionAgentAsync`. Cada agente realiza una llamada independiente con structured output; la aplicación prepara y transfiere el contexto mínimo entre ellas.

La consulta del estado y de la base local es determinista. La aplicación registra un incidente sólo si esos datos no contienen ya un incidente conocido ni una solución. Este proyecto usa Microsoft.Extensions.AI y no usa Microsoft Agent Framework.

Desde la raíz del repositorio:

```powershell
dotnet run --project .\labs\lab-15-multi-agent\lab-15a-multi-agent-explicito\Lab15a.MultiAgentExplicito.csproj
```

Requiere .NET 10, conectividad con el modelo y la configuración del `.env` raíz. La guía del [Lab 15](../README.md) explica el flujo, los escenarios y la comparación con MAF.
