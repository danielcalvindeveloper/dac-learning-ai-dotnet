# Lectura opcional - `LabConsole`

## ¿Por qué existe?

`LabConsole` agrupa únicamente la presentación de información en consola.

No llama al modelo y no decide qué proveedor utilizar.

Su objetivo es evitar que el flujo principal se llene de:

```csharp
Console.WriteLine(...)
```

que no aportan al concepto central del laboratorio.

---

## `WriteError()`

```csharp
public static void WriteError(string message)
```

escribe errores mediante:

```csharp
Console.Error
```

La intención es diferenciar una salida normal de un problema de configuración o ejecución.

---

## `WriteRequest()`

```csharp
public static void WriteRequest(
    LabConfiguration configuration,
    string prompt)
```

muestra:

```text
Proveedor
Modelo
Prompt
```

antes de ejecutar la solicitud.

No modifica ningún dato.

Sólo hace visible qué estamos enviando y con qué configuración.

---

## `WriteResponse()`

```csharp
public static void WriteResponse(string response)
```

muestra el texto generado por el modelo.

El método es deliberadamente pequeño:

```csharp
Console.WriteLine(response);
```

---

## ¿Era necesaria esta clase?

No.

El programa funcionaría perfectamente escribiendo todo directamente en `Program.cs`.

Existe por una razón pedagógica:

```text
Program.cs
    ↓
mostrar el concepto

LabConsole
    ↓
mostrar información al usuario
```

---

## Idea principal

`LabConsole` no agrega capacidad.

Sólo mantiene la presentación fuera del camino principal de aprendizaje.
