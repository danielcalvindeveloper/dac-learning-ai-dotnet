# Structured Output

## El problema

Hasta cierto punto podemos trabajar con:

```csharp
string
```

Pero muchas aplicaciones necesitan datos:

```text
nombre
edad
categoría
importe
fecha
```

No queremos interpretar manualmente una oración cada vez.

## Respuesta tipada

`Microsoft.Extensions.AI` permite pedir una respuesta asociada a un tipo:

```csharp
ChatResponse<Persona>
```

mediante:

```csharp
GetResponseAsync<Persona>(...)
```

## Flujo

```text
prompt
  ↓
modelo
  ↓
respuesta estructurada
  ↓
Persona / Producto / otro tipo C#
```

## TryGetResult

La aplicación debe contemplar que la respuesta no pueda materializarse correctamente.

Por eso utilizamos:

```csharp
response.TryGetResult(...)
```

## Qué necesidad resuelve

Permite integrar el LLM con código de aplicación que necesita contratos conocidos en lugar de texto libre.

## Dónde aparece

Lab 05 trabaja con `Persona`, `Personas` y `Producto`.
