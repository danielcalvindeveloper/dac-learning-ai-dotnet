# Lectura opcional - Prompt Templates

## ¿Qué problema resuelven?

Sin templates, un servicio puede terminar mezclando:

```text
lógica de aplicación
+
texto detallado del prompt
```

Por ejemplo:

```csharp
string prompt =
    $"Traduce al inglés: {texto}";
```

Funciona, pero cuando los prompts crecen o incorporan más parámetros, esa responsabilidad empieza a ocupar demasiado espacio.

---

## La idea

Un template separa:

```text
estructura fija
```

de:

```text
valores variables
```

Ejemplo:

```text
Traduce al {idioma}.

Texto:
{texto}
```

La estructura no cambia.

Los valores sí.

---

## `PromptTemplates`

En este laboratorio utilizamos una clase estática:

```csharp
public static class PromptTemplates
```

Su responsabilidad es devolver strings.

No llama al modelo.

No conoce servicios.

No conoce configuración.

---

## Raw interpolated strings

C# permite escribir:

```csharp
return $$"""
Traduce al {{idioma}}.

Texto:
{{texto}}
""";
```

El prefijo:

```csharp
$$
```

permite usar interpolación dentro de un raw string.

Con dos signos `$`, la interpolación se expresa mediante:

```csharp
{{variable}}
```

Esto resulta cómodo cuando el prompt ocupa varias líneas.

---

## ¿Por qué no usar una biblioteca?

Porque todavía no existe una necesidad suficiente.

Una biblioteca podría ser útil si necesitáramos:

- templates externos;
- versionado;
- composición compleja;
- motores de plantillas;
- recursos administrados.

Pero para este laboratorio agregaría más conceptos que valor.

---

## Idea principal

```text
PromptTemplates
    no es un nuevo servicio de IA

PromptTemplates
    es una pieza que construye texto
```

El servicio continúa siendo `Assistant`.
