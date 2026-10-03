# Lectura opcional - Configuración y `AiClientFactory`

## Continuidad

Lab 05 reutiliza exactamente el mecanismo de configuración de Labs 03 y 04.

```text
.env
  ↓
LabConfiguration
  ↓
AiClientFactory
  ↓
IChatClient
```

Structured Output no requiere cambiar la forma en que configuramos el cliente.

---

## Variables

```text
AI_PROVIDER
AI_MODEL
AI_API_KEY
AI_URL
```

`AI_URL` es opcional.

Las demás son obligatorias.

---

## `LabConfiguration`

Carga el `.env`, valida las variables obligatorias y convierte `AI_URL` en:

```csharp
Uri?
```

Una URL inválida produce:

```csharp
InvalidOperationException
```

---

## `AiClientFactory`

Su única responsabilidad es:

```text
configuración válida
      ↓
cliente concreto
      ↓
IChatClient
```

No conoce:

```text
Persona
Producto
Structured Output
PromptTemplates
```

Eso mantiene la infraestructura separada del concepto nuevo.

---

## Idea principal

Si una pieza ya fue resuelta en un laboratorio anterior y no necesita cambiar:

```text
la reutilizamos
```

No volvemos a convertirla en protagonista.
