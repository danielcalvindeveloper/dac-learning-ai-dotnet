# Lab 00 - Introducción a los artefactos de Fundamentos

## Objetivo

Este laboratorio es una **rampa de entrada opcional** al taller.

Antes de comenzar el recorrido incremental vamos a hacer algo deliberadamente simple:

```text
escribir una pregunta
        ↓
enviarla a un modelo
        ↓
ver la respuesta
```

No hay `.env`, factory, Dependency Injection ni helpers.

La intención es que la primera experiencia sea:

```text
editar una API key
        ↓
dotnet run
        ↓
funciona
```

Después, en los Labs 01–06, iremos incorporando las abstracciones y prácticas que permiten convertir ese ejemplo mínimo en código más estructurado.

> Si ya conocés estas piezas, podés saltar directamente al Lab 01. Ningún laboratorio posterior requiere haber ejecutado Lab 00.

---

## Requisito

Tener instalado el SDK utilizado por el proyecto:

```text
.NET 10
```

Podés verificarlo con:

```powershell
dotnet --version
```

---

# Ejemplo 1 - OpenAI

Entrá al directorio:

```powershell
cd labs\lab-00-introduccion\openai
```

Abrí `Program.cs` y reemplazá:

```csharp
const string apiKey = "TU_API_KEY";
```

por una clave válida.

Luego ejecutá:

```powershell
dotnet run
```

El flujo completo es:

```text
prompt
  ↓
OpenAI.Chat.ChatClient
  ↓
IChatClient
  ↓
modelo
  ↓
ChatResponse
  ↓
response.Text
```

El `Program.cs` está comentado línea por línea para explicar para qué existe cada elemento.

---

# Ejemplo 2 - Gemini

Entrá al directorio:

```powershell
cd labs\lab-00-introduccion\gemini
```

Reemplazá:

```csharp
const string apiKey = "TU_API_KEY";
```

por una clave válida y ejecutá:

```powershell
dotnet build
dotnet run
```

El ejemplo agrega una pieza respecto de OpenAI:

```text
Endpoint
```

porque en este caso utilizamos el endpoint compatible con OpenAI expuesto por Gemini.

El resto del flujo termina nuevamente en:

```text
IChatClient
```

---

# Lo importante de ejecutar ambos ejemplos

OpenAI y Gemini son proveedores diferentes.

Sin embargo, desde nuestra aplicación ambos terminan exponiéndose mediante:

```csharp
IChatClient
```

Conceptualmente:

```text
OpenAI ──┐
         ├──> IChatClient ──> nuestra aplicación
Gemini ──┘
```

Esta idea será central durante la etapa **Fundamentos**.

---

# ¿Por qué la API key está escrita en el código?

Únicamente para mantener estos dos ejemplos mínimos.

No es una práctica recomendada para una aplicación real ni para el resto del taller.

**No subas una API key real a Git.**

A partir del Lab 01 la configuración se externaliza mediante el `.env` común del proyecto.

La diferencia es deliberada:

```text
Lab 00
¿qué es lo mínimo necesario para llamar a un modelo?

Lab 01+
¿cómo empezamos a organizarlo correctamente?
```

---

# Las piezas que acabamos de usar

| Pieza | Para qué aparece |
|---|---|
| modelo | motor que genera la respuesta |
| proveedor | servicio que ofrece acceso al modelo |
| cliente concreto | SDK que sabe comunicarse con ese proveedor |
| `IChatClient` | contrato común para interactuar con modelos de chat |
| prompt | mensaje enviado al modelo |
| `ChatResponse` | respuesta obtenida desde `IChatClient` |
| `response.Text` | texto generado por el modelo |
| endpoint | dirección del servicio al que se conecta el cliente |
| API key | credencial que autoriza la llamada |

---

# Lectura opcional

Los siguientes documentos explican las piezas que aparecerán durante la sección **Fundamentos** del roadmap.

Podés leerlos ahora o volver a ellos a medida que avances.

1. [`docs/01-microsoft-extensions-ai.md`](docs/01-microsoft-extensions-ai.md)  
   Qué problema resuelve `Microsoft.Extensions.AI` y cuáles son sus abstracciones principales.

2. [`docs/02-ichatclient-y-clientes.md`](docs/02-ichatclient-y-clientes.md)  
   Diferencia entre cliente concreto, adaptador e `IChatClient`.

3. [`docs/03-mensajes-y-respuestas.md`](docs/03-mensajes-y-respuestas.md)  
   `ChatMessage`, roles, `ChatResponse` e historial.

4. [`docs/04-dependency-injection.md`](docs/04-dependency-injection.md)  
   Por qué aparece DI y qué resuelven `ServiceCollection`, `Singleton` y `Transient`.

5. [`docs/05-prompt-templates.md`](docs/05-prompt-templates.md)  
   Cómo pasar de strings fijos a prompts parametrizados.

6. [`docs/06-structured-output.md`](docs/06-structured-output.md)  
   Cómo pasar de texto libre a objetos C# tipados.

7. [`docs/07-memoria-conversacional.md`](docs/07-memoria-conversacional.md)  
   Historial, sesión, store y ventana conversacional.

8. [`docs/08-configuracion.md`](docs/08-configuracion.md)  
   Por qué la configuración real se externaliza y cuál será el contrato común del proyecto.

---

# Alcance

Lab 00 sólo cubre los artefactos de **Fundamentos**.

No intenta explicar todavía:

- embeddings;
- documentos;
- vector stores;
- RAG;
- tools;
- agentes;
- workflows;
- MCP.

Esos conceptos aparecen más adelante en el roadmap y tendrán su propio contexto.

---

# Siguiente paso

**Lab 01 - Hello LLM**

Ahora que sabemos que una llamada puede ser muy pequeña, el siguiente laboratorio empieza el recorrido formal del taller y externaliza la configuración sin perder de vista el flujo esencial:

```text
prompt
  ↓
IChatClient
  ↓
modelo
  ↓
respuesta
```
