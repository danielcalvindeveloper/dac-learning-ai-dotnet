# Lectura opcional - Singleton y ciclos de vida

## El store como Singleton

Registramos:

```csharp
services.AddSingleton<
    IChatMemoryStore,
    InMemoryChatMemoryStore>();
```

Esto garantiza una instancia compartida durante la vida del contenedor.

---

## ¿Por qué importa?

Imaginemos que fuera Transient.

Cada resolución podría obtener:

```text
nuevo store
```

Entonces:

```text
primera llamada → memoria A
segunda llamada → memoria B
```

y el contexto podría perderse entre instancias.

---

## Con Singleton

Todas las instancias de `Assistant` reciben:

```text
el mismo store
```

Por eso pueden consultar las mismas sesiones.

---

## `Assistant` sigue siendo Transient

```csharp
services.AddTransient<
    IAssistant,
    Assistant>();
```

Esto sigue siendo razonable porque `Assistant`:

- no guarda directamente el historial;
- no es dueño del estado;
- sólo conserva referencias a sus dependencias.

El estado conversacional está en el store.

---

## `IChatClient` también es Singleton

Al igual que en Labs 03–05:

```csharp
services.AddSingleton<IChatClient>(...)
```

reutilizamos el mismo cliente configurado durante la ejecución.

---

## Lo importante

No memorizar:

```text
store siempre Singleton
assistant siempre Transient
```

La idea correcta es:

```text
el ciclo de vida debe acompañar
la responsabilidad y el estado del componente
```

En este laboratorio las elecciones hacen visible esa relación.
