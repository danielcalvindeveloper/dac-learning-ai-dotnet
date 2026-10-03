# Dependency Injection en el recorrido

## Qué problema resuelve

Si una clase crea por sí misma todas sus dependencias:

```csharp
new ChatClient(...)
```

queda fuertemente acoplada a esa forma concreta de construcción.

Con inyección por constructor una clase declara qué necesita:

```csharp
public Assistant(IChatClient chatClient)
```

pero no decide cómo crearlo.

## ServiceCollection

El contenedor se configura mediante registros como:

```csharp
services.AddSingleton<IChatClient>(...);
services.AddTransient<IAssistant, Assistant>();
```

## Singleton

Una única instancia se reutiliza durante la vida del contenedor.

En el taller lo usamos cuando tiene sentido compartir un recurso, por ejemplo un cliente o un memory store en memoria.

## Transient

Se crea una nueva instancia cada vez que se resuelve el servicio.

## Qué resuelve el contenedor

```text
IAssistant
    ↓
Assistant
    ↓ necesita
IChatClient
```

El contenedor conoce los registros y construye el grafo.

## Dónde aparece

Lab 03 introduce DI explícitamente para separar infraestructura y lógica de aplicación.
