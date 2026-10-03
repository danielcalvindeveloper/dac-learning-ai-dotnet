# Lectura opcional - Dependency Injection en .NET

## ¿Qué problema resuelve?

Supongamos que tenemos:

```text
Assistant necesita IChatClient
```

Sin un contenedor podríamos construir manualmente:

```csharp
IChatClient chatClient = ...;
IAssistant assistant =
    new Assistant(chatClient);
```

Eso es válido.

Dependency Injection no crea una capacidad nueva.

Automatiza la composición de objetos y sus dependencias.

---

## El contenedor de .NET

Creamos una colección:

```csharp
ServiceCollection services = new();
```

Luego registramos relaciones:

```csharp
services.AddSingleton<IChatClient>(...);

services.AddTransient<IAssistant, Assistant>();
```

Estas líneas le dicen al contenedor:

```text
si alguien pide IChatClient
    sé cómo obtenerlo

si alguien pide IAssistant
    usa Assistant
```

---

## Resolución

Cuando hacemos:

```csharp
IAssistant assistant =
    serviceProvider.GetRequiredService<IAssistant>();
```

el contenedor inspecciona el constructor de `Assistant`:

```csharp
public Assistant(IChatClient chatClient)
```

y descubre otra dependencia:

```text
IChatClient
```

Como también está registrada, puede construir:

```text
IAssistant
    ↓
Assistant
    ↓
IChatClient
```

---

## Inyección por constructor

La dependencia entra por:

```csharp
public Assistant(IChatClient chatClient)
```

Esto hace visible qué necesita la clase para funcionar.

No hay que buscar:

```text
variables globales
service locator
new escondidos
```

La dependencia está declarada en el constructor.

---

## DI no es el servicio

Este punto es fundamental para el Lab 03.

```text
IAssistant / Assistant
    = diseño del servicio

Dependency Injection
    = mecanismo para construirlo
```

Podríamos cambiar el mecanismo de construcción y el concepto de servicio seguiría siendo el mismo.

---

## `BuildServiceProvider()`

```csharp
using ServiceProvider serviceProvider =
    services.BuildServiceProvider();
```

construye el contenedor a partir de los registros realizados.

A partir de ahí puede resolver las dependencias.

---

## Idea principal

DI responde:

> ¿Quién construye los objetos y les entrega lo que necesitan?

El servicio responde:

> ¿Qué capacidad ofrece nuestra aplicación?

Son conceptos relacionados, pero no son lo mismo.
