# Lectura opcional - `IAssistant` y `Assistant`

## El objetivo de este documento

El Lab 03 introduce una idea nueva:

```text
servicio de aplicación
```

Antes de pensar en Dependency Injection conviene entender por qué existen:

```text
IAssistant
Assistant
```

---

## El problema

Sin un servicio, `Program.cs` podría terminar repitiendo en distintos lugares:

```text
crear o recibir IChatClient
construir prompt
hacer llamada
extraer respuesta
```

Además, el programa tendría que conocer detalles como:

```text
"Traduce al inglés: ..."
"Resume en una frase: ..."
```

Eso mezcla intención con implementación.

---

## `IAssistant`: el contrato

```csharp
public interface IAssistant
{
    Task<string> ChatAsync(string message);
    Task<string> TraducirAsync(string texto);
    Task<string> ResumirAsync(string texto);
}
```

La interfaz expresa capacidades.

Puede leerse como:

```text
el sistema sabe conversar
el sistema sabe traducir
el sistema sabe resumir
```

No explica cómo se implementan.

---

## ¿Por qué una interfaz?

En este laboratorio la interfaz cumple principalmente dos objetivos.

### 1. Expresar un contrato

El consumidor conoce:

```text
qué puede pedir
```

sin necesitar conocer:

```text
cómo se resuelve
```

### 2. Desacoplar al consumidor de la implementación

`Program.cs` trabaja con:

```csharp
IAssistant
```

y no necesita crear directamente:

```csharp
new Assistant(...)
```

La implementación puede evolucionar sin modificar el contrato mientras conserve las mismas operaciones.

---

## `Assistant`: la implementación

`Assistant` contiene la lógica concreta.

Por ejemplo:

```csharp
public async Task<string> TraducirAsync(string texto)
{
    string prompt =
        $"Traduce al inglés: {texto}";

    ChatResponse response =
        await _chatClient.GetResponseAsync(prompt);

    return response.Text;
}
```

La responsabilidad queda claramente separada:

```text
Program.cs
    pide traducir

Assistant
    sabe cómo traducir usando IA
```

---

## La dependencia `IChatClient`

`Assistant` necesita hablar con un modelo.

Por eso recibe:

```csharp
IChatClient
```

mediante el constructor.

```csharp
public Assistant(IChatClient chatClient)
{
    _chatClient = chatClient;
}
```

La clase no hace:

```csharp
new ChatClient(...)
```

Eso sería mezclar dos responsabilidades:

```text
usar el cliente
+
construir el cliente
```

---

## ¿Es `Assistant` un agente?

No.

En este laboratorio `Assistant` es simplemente:

```text
un servicio de aplicación
```

con operaciones explícitas.

No decide objetivos, no selecciona herramientas y no ejecuta ciclos autónomos.

El nombre `Assistant` no implica comportamiento agéntico.

---

## Idea principal

```text
IAssistant
    define qué ofrecemos

Assistant
    define cómo lo hacemos

IChatClient
    es una dependencia necesaria para hacerlo
```

Dependency Injection aparecerá después para conectar esas piezas.
