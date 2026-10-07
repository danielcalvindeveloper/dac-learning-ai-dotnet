# Lectura opcional - Dependencias del servicio

El constructor hace visibles los colaboradores:

```csharp
public RagService(
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    InMemoryVectorStore vectorStore)
```

```text
RagService
  ├── IChatClient
  ├── IEmbeddingGenerator
  └── InMemoryVectorStore
```

Esto es inyección por constructor: el servicio usa dependencias que recibe del exterior. No necesita conocer credenciales, endpoints ni constructores de SDKs. Las factories construyen clientes y `LabConfiguration` valida las variables.

El contenedor de DI de .NET, ya estudiado anteriormente, realiza la composición. El concepto central de 09b sigue siendo encapsular una responsabilidad de aplicación.

Clientes y store son Singleton durante la vida del `ServiceProvider`. El store debe conservar el índice entre `IndexDocumentAsync` y `AskAsync`, incluso si se resuelve otra instancia del servicio.

`IRagService` se registra como Transient porque `RagService` coordina sin mantener un índice propio. Cada instancia recibe el mismo store del contenedor. Es una elección coherente con este ejemplo, no una regla universal de ciclos de vida.

Al finalizar `Program.cs`, el `using` del proveedor de servicios libera los clientes que construyó. No agregamos una interfaz para el store: una dependencia concreta también puede inyectarse por constructor.

[Volver al laboratorio](../README.md).
