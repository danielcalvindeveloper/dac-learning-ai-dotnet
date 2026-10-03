# Memoria conversacional

## Historial

El modelo conoce el contexto que recibe en la llamada actual.

Para continuar una conversación la aplicación vuelve a enviar mensajes anteriores.

```text
user
assistant
user
assistant
```

## Una conversación

Lab 02 mantiene una colección de mensajes y muestra la diferencia entre llamadas aisladas e historial.

## Varias conversaciones

Cuando existen varias sesiones necesitamos aislar sus historiales:

```text
sesion-a → Daniel
sesion-b → Roberto
```

## Memory Store

Lab 06 introduce un contrato:

```csharp
IChatMemoryStore
```

Su responsabilidad es almacenar y recuperar mensajes según un `sessionId`.

```text
sessionId
   ↓
IChatMemoryStore
   ↓
historial de esa sesión
```

## Ventana

El historial no debería crecer indefinidamente.

Por eso el laboratorio mantiene una cantidad máxima de mensajes y elimina los más antiguos.

## Idea principal

```text
memoria conversacional
=
gestión del contexto que la aplicación vuelve a suministrar al modelo
```
