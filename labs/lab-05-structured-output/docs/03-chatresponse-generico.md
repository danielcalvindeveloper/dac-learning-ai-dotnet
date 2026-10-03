# Lectura opcional - `ChatResponse<T>`

## `GetResponseAsync<T>()`

Hasta ahora utilizábamos:

```csharp
ChatResponse response =
    await _chatClient.GetResponseAsync(prompt);
```

y obteníamos:

```csharp
response.Text
```

En Lab 05 utilizamos:

```csharp
ChatResponse<Persona> response =
    await _chatClient.GetResponseAsync<Persona>(prompt);
```

El parámetro genérico:

```csharp
<Persona>
```

indica el tipo de resultado esperado.

---

## `ChatResponse<T>`

Podemos pensar:

```text
ChatResponse
    respuesta de chat general

ChatResponse<T>
    respuesta de chat que además puede producir T
```

---

## `TryGetResult`

Para obtener el valor tipado:

```csharp
response.TryGetResult(
    out T? result)
```

El patrón `Try...` permite comprobar si el resultado está disponible antes de usarlo.

```text
true
  ↓
hay un T utilizable

false
  ↓
no hay un resultado compatible
```

---

## `GetResultOrThrow<T>`

El laboratorio encapsula esa repetición:

```csharp
private static T GetResultOrThrow<T>(
    ChatResponse<T> response)
```

Así:

```csharp
ExtraerPersonaAsync
ExtraerPersonasAsync
ExtraerProductoAsync
```

comparten una única validación.

---

## ¿Por qué lanzar una excepción?

Porque los métodos prometen devolver:

```text
Persona
Personas
Producto
```

No prometen:

```text
T?
```

Si el contrato no puede cumplirse, el método falla explícitamente.

Eso es preferible a devolver silenciosamente un objeto inexistente.

---

## Idea principal

```text
GetResponseAsync<T>()
    solicita el tipo

TryGetResult()
    comprueba el resultado

GetResultOrThrow<T>()
    centraliza la decisión del servicio
```
