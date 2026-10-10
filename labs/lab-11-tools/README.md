# Lab 11 - Tools

## Objetivo

Comprender cómo un LLM solicita una función de nuestra aplicación y después delegar la coordinación de ese intercambio a una abstracción existente de `Microsoft.Extensions.AI`.

Lab 11 está compuesto por dos experiencias sobre el mismo caso: `CalcularTotalConIva` y una compra de $1000 con IVA del 21%.

## Estado

✅ Implementado mediante dos sublaboratorios ejecutables e independientes:

| Sublaboratorio | Propósito | Estado |
|---|---|---|
| [11a - Primer Tool explícito](lab-11a-tool-explicita/README.md) | Comprender el mecanismo de Tool Calling | ✅ Implementado |
| [11b - Invocación automática de Tools](lab-11b-tool-invocation/README.md) | Utilizar `FunctionInvokingChatClient` para coordinar el mismo ciclo | ✅ Implementado |

M6 continúa 🚧 En progreso porque Lab 12 sigue pendiente.

## Por qué hay dos sublabs

```text
11a: vemos el protocolo
        ↓
11b: dejamos que Microsoft.Extensions.AI gestione el protocolo
```

Primero entender, después abstraer y finalmente integrar. 11a permite observar las solicitudes, los argumentos, la ejecución y el resultado. 11b encapsula esa responsabilidad cuando ya comprendemos el problema que resuelve.

11a es deliberadamente explícito. El cambio a 11b conserva el ejercicio y cambia quién coordina el intercambio de mensajes.

## Qué responsabilidad cambia

```text
11a — explícito
Aplicación
├── recibe Function Call
├── interpreta argumentos
├── ejecuta Tool
├── crea Function Result
└── continúa conversación

11b — abstracción
Aplicación
├── define Tool
├── registra Tool
└── realiza la consulta

Microsoft.Extensions.AI
├── gestiona Function Call
├── invoca la Tool dentro de la aplicación
├── gestiona Function Result
└── continúa conversación
```

| Responsabilidad | 11a explícito | 11b automático |
|---|---|---|
| Definir Tool | Aplicación | Aplicación |
| Informar Tools al modelo | Aplicación | Aplicación |
| Decidir solicitar la Tool | LLM | LLM |
| Interpretar Function Call | Aplicación | Microsoft.Extensions.AI |
| Ejecutar método C# | Aplicación | Aplicación, coordinada por Microsoft.Extensions.AI |
| Crear Function Result | Aplicación | Microsoft.Extensions.AI |
| Continuar conversación | Aplicación | Microsoft.Extensions.AI |
| Generar la respuesta final | LLM | LLM |

En ambos casos **el LLM solicita y la aplicación ejecuta**. Automatizar la invocación no significa ejecutar código dentro del modelo ni convierte el ejercicio en un agente.

## Estructura

```text
lab-11-tools/
├── README.md
├── lab-11a-tool-explicita/
│   ├── docs/
│   ├── Lab11a.ToolExplicita.csproj
│   ├── Program.cs
│   └── ...
└── lab-11b-tool-invocation/
    ├── docs/
    ├── Lab11b.ToolInvocation.csproj
    ├── Program.cs
    └── ...
```

La carpeta padre contiene esta guía. Cada sublab tiene su propio proyecto, configuración, factory y documentación. Ejecutá `dotnet run` desde el sublab que quieras estudiar, siguiendo su README.

## Configuración y compatibilidad

Ambos utilizan el `.env` raíz con `AI_PROVIDER`, `AI_MODEL`, `AI_API_KEY` y `AI_URL`. `AI_EMBEDDING_MODEL` puede permanecer configurado para otros labs; aquí no se utiliza.

El modelo y su endpoint deben soportar Tool Calling. La abstracción reduce la coordinación manual del protocolo, pero no garantiza compatibilidad absoluta con todos los proveedores.

### Validación y compatibilidad

Ambos proyectos completaron restore, build y ejecución real con el Gemini configurado. En ambos se ejecutó `CalcularTotalConIva(1000, 21)`, se obtuvo `1210` y el modelo devolvió una respuesta final.

La representación común facilita trabajar uniformemente, pero la respuesta nativa puede contener metadata adicional necesaria para continuar el protocolo. Ambos sublabs conservan esa representación según el tipo recibido del SDK OpenAI, sin condiciones por proveedor: 11a lo hace explícitamente y 11b lo encapsula en el cliente base.

Gemini y `thought_signature` fueron el caso real que hizo visible la necesidad, también en la prueba inicial de 11b. No se trata de completar campos de Gemini: reutilizamos el mensaje nativo disponible. Otra integración basada en un SDK diferente podría requerir otro tratamiento. El [README de 11b](lab-11b-tool-invocation/README.md#validación-y-compatibilidad) explica el hallazgo y la solución comprobada.

## Qué aprendemos

Al completar las dos experiencias deberías poder explicar qué son Tool, Function Call y Function Result; dónde se ejecuta el método C#; qué complejidades aparecen al coordinar el ciclo y qué responsabilidad encapsula `FunctionInvokingChatClient`.

```text
11a: entendimos el mecanismo
    ↓
11b: encapsulamos el mecanismo
    ↓
12: trabajaremos con múltiples capacidades
```

## Siguiente laboratorio

[Lab 12 - Function Calling](../lab-12-function-calling/README.md) continúa ⏳ Pendiente. Avanzará hacia múltiples Tools, selección, argumentos, errores, casos sin Tool y una Tool que encapsule una capacidad externa. La invocación automática de una única Tool ya se estudia en 11b.

[Roadmap](../../ROADMAP.md) · [Fundamento pedagógico](../../docs/04-enfoque-pedagogico-y-evolucion-del-roadmap.md).
