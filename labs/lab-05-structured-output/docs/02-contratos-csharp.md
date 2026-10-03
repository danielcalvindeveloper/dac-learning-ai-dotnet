# Lectura opcional - Tipos C# como contratos

## `Persona`

```csharp
public sealed class Persona
{
    public string Nombre { get; init; } = string.Empty;
    public int Edad { get; init; }
}
```

Este tipo declara:

```text
Nombre → string
Edad   → int
```

Eso es más preciso que pedir:

```text
"devuelve los datos de una persona"
```

y luego analizar texto libre.

---

## Propiedades con `init`

Utilizamos:

```csharp
{ get; init; }
```

porque estos objetos representan un resultado construido a partir de la respuesta.

Una vez creado, no necesitamos modificarlo continuamente.

---

## Valores iniciales

Por ejemplo:

```csharp
string Nombre { get; init; } = string.Empty;
```

evita dejar una referencia string sin valor inicial en un proyecto con nullable reference types habilitados.

---

## `Personas` como objeto raíz

```csharp
public sealed class Personas
{
    public List<Persona> PersonasEncontradas { get; init; } = [];
}
```

En lugar de utilizar directamente:

```text
List<Persona>
```

como raíz, usamos un objeto contenedor.

Esto aporta:

```text
un nombre claro
una raíz explícita
posibilidad de agregar metadata más adelante
```

Por ejemplo, en otro contexto podría crecer a:

```text
PersonasEncontradas
Cantidad
Origen
```

sin cambiar completamente la forma raíz.

---

## `Producto`

```csharp
public sealed class Producto
{
    public string Nombre { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
}
```

El mismo mecanismo sirve para contratos diferentes.

---

## ¿Son entidades de dominio?

No necesariamente.

En este laboratorio son simplemente:

```text
modelos de salida
```

No agregamos reglas de dominio que todavía no necesitamos.

KISS.
