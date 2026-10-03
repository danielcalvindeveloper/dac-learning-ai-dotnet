# Lectura opcional - Configuración y `AiClientFactory`

## Continuidad

Lab 06 utiliza el mismo mecanismo de configuración que Labs 03–05.

```text
.env
  ↓
LabConfiguration
  ↓
AiClientFactory
  ↓
IChatClient
```

No agregamos una nueva forma de configurar el modelo.

---

## Variables

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
AI_URL
```

Las primeras tres son obligatorias.

`AI_URL` es opcional.

---

## `LabConfiguration`

Carga y valida la configuración.

También valida que `AI_URL`, si existe, sea una URL absoluta válida.

---

## `AiClientFactory`

Construye:

```csharp
IChatClient
```

a partir de esa configuración.

No conoce:

```text
sessionId
IChatMemoryStore
Assistant
```

---

## Separación

```text
configuración
    LabConfiguration

creación del cliente
    AiClientFactory

memoria
    IChatMemoryStore

servicio
    Assistant
```

Cada pieza tiene una responsabilidad concreta.

---

## Idea principal

La memoria conversacional no cambia cómo configuramos el proveedor.

Por eso reutilizamos la infraestructura existente en lugar de volver a explicarla en el flujo principal.
