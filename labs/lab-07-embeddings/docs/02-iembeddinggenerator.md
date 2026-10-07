# Lectura opcional - `IEmbeddingGenerator`

## La abstracción

`Microsoft.Extensions.AI` ofrece:

```csharp
IEmbeddingGenerator<string, Embedding<float>>
```

El primer parámetro expresa el tipo de entrada: textos. El segundo expresa el tipo de embedding generado: un vector con componentes `float`.

## Entrada y resultado

La API utilizada en el laboratorio es:

```csharp
GeneratedEmbeddings<Embedding<float>> embeddings =
    await generator.GenerateAsync([query, documentA, documentB]);

ReadOnlyMemory<float> queryVector = embeddings[0].Vector;
```

`GenerateAsync` recibe una colección de textos y devuelve asincrónicamente una colección de embeddings correspondiente a las entradas. También admite opciones de generación y cancelación; el ejercicio utiliza los argumentos opcionales por defecto.

`Embedding<float>` representa un embedding. Su propiedad `Vector` devuelve `ReadOnlyMemory<float>`. Con `.Span` accedemos a sus componentes sin copiar el vector para calcular similitud.

## Separación del SDK

La factory construye `OpenAI.Embeddings.EmbeddingClient` y lo adapta mediante `AsIEmbeddingGenerator()`.

`Program.cs` depende del contrato común y no necesita conocer constructores, credenciales ni opciones del SDK concreto. La declaración `using` libera el generador al terminar.

La abstracción no garantiza que cualquier endpoint o modelo soporte embeddings. Esa capacidad debe existir en el proveedor configurado.

`IChatClient` representa la capacidad de chat y `IEmbeddingGenerator` la de generar embeddings. La factory utiliza `AI_EMBEDDING_MODEL`; no asume que el modelo de chat de `AI_MODEL` admita esta operación.

## Referencias

- [Guía oficial de `IEmbeddingGenerator`](https://learn.microsoft.com/en-us/dotnet/ai/iembeddinggenerator)
- [Adaptador oficial para OpenAI](https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI.OpenAI/README.md)

Los ejemplos utilizan las APIs compiladas con `Microsoft.Extensions.AI.OpenAI` 10.10.1.
