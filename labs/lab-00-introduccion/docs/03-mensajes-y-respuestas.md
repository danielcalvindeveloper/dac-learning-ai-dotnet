# Mensajes y respuestas

## Un chat no es sólo un string

Una conversación está compuesta por mensajes con roles.

`Microsoft.Extensions.AI` representa esos mensajes mediante:

```csharp
ChatMessage
```

## Roles

Los roles más habituales son:

```text
User       → mensaje del usuario
Assistant  → respuesta del modelo
System     → instrucciones de contexto o comportamiento
```

En código aparecen mediante `ChatRole`.

## ChatResponse

Una llamada simple devuelve:

```csharp
ChatResponse
```

El texto generado puede leerse con:

```csharp
response.Text
```

La respuesta también puede contener mensajes estructurados mediante:

```csharp
response.Messages
```

## Historial

Un modelo no recuerda automáticamente las llamadas independientes de nuestra aplicación.

Para mantener contexto enviamos nuevamente los mensajes anteriores:

```text
user
assistant
user
assistant
```

Eso es la base del Lab 02.

## Idea principal

```text
llamada aislada
    ≠
conversación con historial
```
