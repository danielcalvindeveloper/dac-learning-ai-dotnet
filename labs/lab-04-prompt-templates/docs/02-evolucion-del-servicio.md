# Lectura opcional - Evolución del servicio

## Lab 03

En el Lab 03 introdujimos:

```text
IAssistant
Assistant
```

como servicio de aplicación.

La implementación hacía todo esto:

```text
recibir parámetros
construir prompt
llamar IChatClient
devolver texto
```

---

## Lab 04

Ahora extraemos sólo una responsabilidad:

```text
construir prompt
```

y la movemos a:

```text
PromptTemplates
```

La estructura queda:

```text
IAssistant
    ↓
Assistant
    ├── PromptTemplates
    └── IChatClient
```

---

## Qué sigue perteneciendo a `Assistant`

`Assistant` todavía:

- implementa el contrato;
- decide qué template corresponde a cada operación;
- llama al modelo;
- devuelve el resultado.

Por eso sigue siendo el servicio.

---

## Qué pertenece a `PromptTemplates`

`PromptTemplates`:

- recibe parámetros;
- arma texto;
- devuelve el prompt.

Nada más.

---

## ¿Por qué no crear `IPromptTemplates`?

Porque actualmente no existe una necesidad real.

La clase:

- no mantiene estado;
- no tiene dependencias;
- no necesita reemplazarse durante el ejercicio;
- no representa una capacidad de aplicación.

Agregar:

```text
IPromptTemplates
PromptTemplatesService
registro DI
constructor adicional
```

haría más difícil ver el concepto que queremos enseñar.

---

## KISS

La regla aplicada es:

> abstraer cuando la abstracción resuelve un problema concreto.

No abstraer solamente porque podemos hacerlo.

---

## Resultado

Lab 03 enseñó:

```text
servicios
```

Lab 04 agrega:

```text
separación de prompts
```

sin desarmar el diseño anterior.
