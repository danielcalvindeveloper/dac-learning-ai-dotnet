# Tests y diagnóstico

Las pruebas automatizadas verifican lógica propia con datos controlados. Las comprobaciones manuales verifican la integración real con Qdrant y el proveedor AI. Una categoría no reemplaza a la otra.

## Ejecutar tests

Desde `reference/rag-vector-store/`:

```powershell
dotnet restore
dotnet build
dotnet test
```

[`RagVectorStore.Tests.csproj`](../tests/RagVectorStore.Tests/RagVectorStore.Tests.csproj) referencia la aplicación y xUnit v3. [`global.json`](../global.json) selecciona Microsoft Testing Platform para `dotnet test` dentro de esta referencia. No modifica los labs ni fija un SDK concreto. [Integración oficial de xUnit con Microsoft Testing Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).

En xUnit, `[Fact]` expresa un caso y `[Theory]` ejecuta un caso por conjunto de `[InlineData]`. Por eso la cantidad de tests ejecutados puede ser mayor que la cantidad de métodos. `TestContext.Current.CancellationToken` permite cancelar operaciones asíncronas del test.

## Qué cubre cada archivo

| Archivo | Casos y propósito |
|---|---|
| [AppConfigurationTests.cs](../tests/RagVectorStore.Tests/AppConfigurationTests.cs) | Modelos separados, variables obligatorias sin defaults ocultos, URLs rechazadas, endpoint alternativo y default local de Qdrant |
| [DocumentTextTests.cs](../tests/RagVectorStore.Tests/DocumentTextTests.cs) | Overlap exacto, índices, último chunk, normalización, texto vacío, ventanas inválidas, archivo inexistente y extensiones permitidas |
| [DocumentChunkRecordTests.cs](../tests/RagVectorStore.Tests/DocumentChunkRecordTests.cs) | ID estable al cambiar texto, identidad distinta por documento/posición, mapeo y esquema con dimensión dinámica, Cosine y HNSW |
| [RagServiceTests.cs](../tests/RagVectorStore.Tests/RagServiceTests.cs) | Procedencia, fuentes deduplicadas, ausencia de llamadas al chat sin evidencia y envío exclusivo de fragmentos aceptados |

Los tests de lectura crean archivos temporales y los eliminan en `finally`. Los de configuración pasan un diccionario a `Parse`, sin leer el `.env`. No requieren red, credenciales, Docker ni un LLM.

### RecordingChatClient

El cliente privado de `RagServiceTests` implementa la abstracción existente `IChatClient`. Registra cuántas solicitudes recibe y el prompt enviado; devuelve una respuesta fija. Así podemos afirmar que no hubo llamada cuando faltó evidencia y que no se envió un fragmento descartado.

El texto fijo no simula razonamiento ni prueba la calidad de un LLM. El streaming arroja `NotSupportedException` porque estos tests no usan esa capacidad. No se agregó una interfaz propia sólo para hacer un mock.

## Qué queda fuera de los unit tests

No prueban el protocolo de los SDK, la persistencia real de Qdrant, conectividad, búsqueda remota, calidad de embeddings ni respuestas generadas. Tampoco hay pruebas unitarias específicas para `EmbeddingGeneration`, `IngestionService`, `VectorSearchService` o el manejo de errores de `Program.cs`: sus caminos externos necesitan comprobaciones adicionales.

El README documenta la [validación real realizada](../README.md#validación-realizada) y los [casos manuales](../README.md#cómo-ejecutar-y-casos-manuales). Sus cantidades y resultados corresponden al corpus y configuración indicados allí; no son promesas para todos los modelos.

## Orden para diagnosticar un problema

1. Comprobá el comando y la configuración: el mensaje de variable faltante identifica qué revisar.
2. Comprobá Qdrant con `docker compose ps` y `curl.exe http://localhost:6333/healthz`. El health check usa REST; la aplicación necesita además gRPC en 6334.
3. Si falta la colección, ejecutá `ingest`. Si se rechaza el vector o el esquema, revisá modelo, dimensión y colección usada en la ingesta.
4. Si falla IA, revisá conexión, endpoint, credencial y capacidad del modelo sin publicar la clave.
5. Si la consulta funciona pero responde sin contexto, compará los scores mostrados y el umbral; no lo reduzcas automáticamente para forzar una respuesta.

## Manejo de errores en Program.cs

Los servicios dejan propagarse los errores. El borde de consola los convierte en diagnósticos y códigos de salida:

| Código | Significado |
|---|---|
| 0 | Comando completado; una respuesta sin evidencia también es una finalización válida |
| 1 | Uso incorrecto o error de configuración, archivo, proveedor o store |
| 130 | Operación cancelada mediante Ctrl+C |

El primer `catch` comprueba cancelación. El siguiente recorre `InnerException` para encontrar un `RpcException` que un SDK pudiera envolver y distingue indisponibilidad, timeout, esquema inválido y colección inexistente. Después considera errores AI y errores locales esperados. Los mensajes van a stderr y no vuelcan las respuestas crudas de los SDK.

La consola muestra el contexto aceptado para aprendizaje. Aunque no imprime la API key, sí imprime texto del corpus: si reutilizás el ejemplo con documentos privados, esa salida también contiene información privada.

[Mapa](01-mapa-de-la-solucion.md) · [README y ejecución](../README.md).
