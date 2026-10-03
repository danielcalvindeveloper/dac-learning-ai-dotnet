# Lectura opcional - Structured Output

## El problema

Los modelos de lenguaje generan texto.

Eso es útil para:

```text
explicaciones
resúmenes
conversaciones
```

pero muchas aplicaciones necesitan datos que luego serán procesados por código.

Ejemplo:

```text
Nombre: Daniel
Edad: 63
```

es legible.

Pero:

```csharp
persona.Nombre
persona.Edad
```

es utilizable directamente por la aplicación.

---

## Structured Output

Structured Output permite solicitar una respuesta con una forma conocida.

En este laboratorio esa forma está definida mediante tipos C#.

```text
texto de entrada
      ↓
modelo
      ↓
estructura esperada
      ↓
objeto C#
```

---

## Qué cambia

Sin Structured Output:

```text
modelo → string → interpretar
```

Con Structured Output:

```text
modelo → contrato → objeto
```

La aplicación reduce la necesidad de interpretar manualmente lenguaje natural.

---

## No es sólo JSON

Internamente puede existir una representación estructurada, pero el punto pedagógico del laboratorio no es "usar JSON".

El punto es:

```text
trabajar con tipos conocidos
```

desde C#.

---

## Idea principal

Structured Output convierte una respuesta de IA en algo mucho más cercano a una integración tradicional entre componentes:

```text
entrada
  ↓
operación
  ↓
resultado tipado
```
