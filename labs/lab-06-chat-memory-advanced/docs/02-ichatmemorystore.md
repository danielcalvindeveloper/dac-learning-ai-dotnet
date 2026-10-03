# Lectura opcional - `IChatMemoryStore`

## El contrato

```csharp
public interface IChatMemoryStore
```

define operaciones para:

```text
leer mensajes
agregar un mensaje
agregar varios mensajes
```

---

## ¿Por qué una interfaz?

Porque `Assistant` necesita trabajar con memoria, pero no debería depender de:

```text
Dictionary
```

directamente.

Su necesidad real es:

```text
obtener historial
guardar mensajes
```

---

## Implementación actual

```csharp
InMemoryChatMemoryStore
```

utiliza:

```csharp
Dictionary<string, List<ChatMessage>>
```

La clave es:

```text
sessionId
```

y el valor:

```text
historial de esa sesión
```

---

## `GetMessages()`

Devuelve:

```csharp
IReadOnlyList<ChatMessage>
```

para que el consumidor reciba los mensajes sin necesitar modificar directamente la colección interna.

Además devuelve una copia mediante:

```csharp
messages.ToList()
```

---

## `AddMessage()`

Agrega un mensaje individual.

Se utiliza para el mensaje del usuario.

---

## `AddMessages()`

Agrega una colección.

Se utiliza para incorporar los mensajes devueltos por `ChatResponse`.

---

## Ventana

Después de agregar mensajes se ejecuta:

```csharp
TrimWindow(messages);
```

El límite actual es:

```text
20 mensajes
```

Los más antiguos se eliminan primero.

---

## ¿Por qué la ventana vive en el store?

Porque es una regla sobre:

```text
cómo conservamos historial
```

no sobre:

```text
cómo Assistant llama al modelo
```

Eso mantiene separadas las responsabilidades.

---

## Posibles implementaciones futuras

El mismo contrato podría tener:

```text
DatabaseChatMemoryStore
RedisChatMemoryStore
FileChatMemoryStore
```

sin cambiar `Assistant`.

---

## Idea principal

```text
Assistant conoce memoria
pero no conoce almacenamiento concreto
```
