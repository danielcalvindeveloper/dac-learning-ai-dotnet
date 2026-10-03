# Lectura opcional - `LabConfiguration`

## Objetivo

`LabConfiguration` concentra la carga y validación de la configuración común del laboratorio.

El objetivo es evitar que `Program.cs` tenga que resolver detalles como:

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
AI_URL
```

antes de llegar al tema que realmente queremos estudiar: el historial conversacional.

---

## `Load()`

```csharp
public static LabConfiguration Load()
```

realiza tres tareas:

```text
cargar .env
   ↓
leer variables
   ↓
validarlas
```

La configuración obligatoria es:

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
```

`AI_URL` es opcional.

---

## `GetRequired()`

```csharp
private static string GetRequired(string variableName)
```

centraliza una validación repetitiva:

```text
¿la variable existe y tiene contenido?
```

Si no existe:

```csharp
throw new InvalidOperationException(...)
```

Esto permite que `Program.cs` maneje todos los errores de configuración mediante un único `catch`.

---

## `GetOptionalEndpoint()`

Si `AI_URL` está vacía:

```csharp
return null;
```

Si tiene contenido, se valida con:

```csharp
Uri.TryCreate(...)
```

Una URL inválida también se transforma en:

```csharp
InvalidOperationException
```

para mantener una única vía de error de configuración.

---

## Idea principal

Esta clase no forma parte del aprendizaje sobre memoria conversacional.

Existe para sacar infraestructura del camino principal.
