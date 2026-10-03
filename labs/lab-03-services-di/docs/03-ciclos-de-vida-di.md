# Lectura opcional - Singleton, Transient y ciclos de vida

## ¿Qué es un ciclo de vida?

Cuando registramos un servicio debemos decidir cuánto tiempo vive su instancia.

En este laboratorio aparecen dos ciclos:

```text
Singleton
Transient
```

---

## Singleton

Registramos:

```csharp
services.AddSingleton<IChatClient>(
    _ => AiClientFactory.CreateFromEnvironment());
```

`Singleton` significa que el contenedor crea una instancia y la reutiliza durante toda la vida del contenedor.

Conceptualmente:

```text
primera solicitud
      ↓
crear IChatClient
      ↓
guardar instancia
      ↓
reutilizarla
```

---

## ¿Por qué `IChatClient` como Singleton?

En este laboratorio:

- existe una sola configuración activa;
- utilizamos el mismo proveedor y modelo;
- el cliente puede reutilizarse para varias operaciones;
- no necesitamos crear un cliente para cada llamada.

Por eso tiene sentido:

```text
una configuración
      ↓
un cliente reutilizable
```

Además evita convertir cada operación:

```text
ChatAsync
TraducirAsync
ResumirAsync
```

en una excusa para volver a construir infraestructura.

---

## Transient

Registramos:

```csharp
services.AddTransient<IAssistant, Assistant>();
```

`Transient` significa que cada resolución puede crear una nueva instancia.

```text
GetRequiredService<IAssistant>()
      ↓
nuevo Assistant
```

---

## ¿Por qué `Assistant` puede ser Transient?

La clase actual es liviana.

Sólo conserva:

```csharp
private readonly IChatClient _chatClient;
```

y no mantiene estado conversacional.

Por eso crear una instancia nueva no tiene un costo o significado especial en este laboratorio.

---

## ¿Es obligatorio que sea Transient?

No.

Es importante no convertir el ejemplo en una regla universal.

El ciclo de vida depende de la responsabilidad real del servicio.

Si `Assistant` almacenara estado, recursos u otras dependencias, la decisión podría cambiar.

---

## ¿Y Scoped?

.NET también dispone de:

```text
Scoped
```

Es especialmente común en aplicaciones web, donde una instancia puede vivir durante una solicitud HTTP.

No lo necesitamos en esta aplicación de consola.

Introducirlo en el flujo principal sólo agregaría ruido.

---

## Regla pedagógica del Lab 03

No memorices:

```text
IChatClient siempre Singleton
Assistant siempre Transient
```

Recordá:

```text
el ciclo de vida expresa cuánto tiempo
queremos reutilizar una instancia
```

En este laboratorio las elecciones son simples y adecuadas al ejemplo.
