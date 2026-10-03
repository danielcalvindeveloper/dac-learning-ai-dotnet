# Lectura opcional - `sessionId` y memoria

## ¿Por qué aparece `sessionId`?

En Lab 02 una lista representaba una única conversación.

Eso alcanza mientras existe solamente:

```text
una conversación activa
```

Cuando aparecen varias, necesitamos identificar cuál historial corresponde a cada mensaje.

---

## `sessionId`

Un valor como:

```text
sesion-a
```

no es memoria por sí mismo.

Es una clave.

Permite hacer:

```text
sessionId
   ↓
buscar historial
```

---

## Aislamiento

Si tenemos:

```text
sesion-a → Daniel
sesion-b → Roberto
```

cada consulta debe recuperar solamente el historial de su propia sesión.

Eso evita:

```text
mezclar contexto
```

---

## ¿Es un usuario?

No necesariamente.

Una persona podría tener varias conversaciones.

Por eso conviene pensar:

```text
sessionId = conversación
```

y no:

```text
sessionId = usuario
```

---

## Idea principal

La memoria conversacional necesita una forma de responder:

> ¿A qué conversación pertenece este mensaje?

En este laboratorio esa respuesta es `sessionId`.
