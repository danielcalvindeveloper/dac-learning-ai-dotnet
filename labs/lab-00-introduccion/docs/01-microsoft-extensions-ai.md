# Microsoft.Extensions.AI

## Qué necesidad resuelve

Los proveedores de modelos ofrecen SDKs y APIs diferentes. Si toda la aplicación conoce esas APIs concretas, cambiar de proveedor o reutilizar lógica obliga a modificar muchas piezas.

`Microsoft.Extensions.AI` introduce **abstracciones comunes** para trabajar con capacidades de IA desde .NET.

```text
Aplicación
    ↓
abstracción común
    ↓
cliente / proveedor concreto
```

## Elementos que utilizaremos

Durante Fundamentos aparecen principalmente:

- `IChatClient`: contrato común para chat;
- `ChatMessage`: representa un mensaje;
- `ChatRole`: indica el rol de un mensaje;
- `ChatResponse`: representa la respuesta del modelo;
- `ChatResponse<T>`: respuesta estructurada y tipada.

Más adelante, fuera de Fundamentos, aparecerá `IEmbeddingGenerator`.

## Qué NO es

`Microsoft.Extensions.AI` no es un modelo ni un proveedor.

```text
Microsoft.Extensions.AI   → abstracciones
OpenAI / Gemini           → proveedores
ChatClient                → cliente concreto
modelo                    → modelo que genera la respuesta
```

## Por qué lo usamos

El proyecto quiere enseñar conceptos que sobrevivan a cambios de SDK y proveedor.

Por eso el código de aplicación intenta depender de abstracciones como:

```csharp
IChatClient
```

en lugar de propagar por toda la solución una clase concreta del proveedor.

## Dónde aparece en el recorrido

- Lab 00: primera observación de `IChatClient`;
- Lab 01: primera llamada formal;
- Lab 02: mensajes e historial;
- Lab 03: DI;
- Lab 05: structured output;
- Lab 06: memoria por sesión.
