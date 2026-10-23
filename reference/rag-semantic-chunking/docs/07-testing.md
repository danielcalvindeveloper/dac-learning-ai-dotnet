# Testing y diagnóstico

## Suite offline

Desde la carpeta de la referencia:

```powershell
dotnet restore
dotnet build
dotnet test
```

[RagSemanticChunking.Tests](../tests/RagSemanticChunking.Tests/RagSemanticChunking.Tests.csproj) usa xUnit v3 y Microsoft Testing Platform, igual que Hybrid. No requiere `.env`, red, Docker ni LLM. Nullable y warnings como errores están activos; no se silencian analizadores.

| Archivo | Qué verifica |
|---|---|
| [ChunkingTests.cs](../tests/RagSemanticChunking.Tests/ChunkingTests.cs) | Párrafos, oraciones, máximo, overlap, Unicode, cobertura, headings, listas, fallback, vacío y cancelación |
| [SemanticTests.cs](../tests/RagSemanticChunking.Tests/SemanticTests.cs) | Vectores controlados, secuencia de cosenos, threshold estricto, mínimo/máximo, cola y respuesta de embeddings inválida |
| [LlmTests.cs](../tests/RagSemanticChunking.Tests/LlmTests.cs) | JSON Schema real, límites factibles, JSON malformado, gaps, overlaps, duplicados, orden, fuera de rango, cobertura, tamaños y rechazo sin fallback |
| [ExperimentTests.cs](../tests/RagSemanticChunking.Tests/ExperimentTests.cs) | CLI, modelos/configuración, IDs/payload, manifiestos, comparabilidad, Recall/MRR, reporte, contexto y ausencia de llamada a chat sin hits |

`ControlledGenerator` y `RecordingChat` implementan abstracciones existentes de MEAI. No agregamos interfaces de un único servicio para mockear. Los tests del LLM prueban propuesta/validación, no que un proveedor produzca siempre el mismo texto. La política de transporte se delega al SDK; no se intenta probar internamente ese SDK.

## Integración separada

Con Qdrant iniciado:

```powershell
dotnet test --project tests/RagSemanticChunking.IntegrationTests
```

El [proyecto separado](../tests/RagSemanticChunking.IntegrationTests/RagSemanticChunking.IntegrationTests.csproj) queda fuera de la solución offline. [QdrantTests](../tests/RagSemanticChunking.IntegrationTests/QdrantTests.cs) ejecuta las cuatro estrategias, ingesta/reingesta, upsert, metadata, búsqueda, comparación y rechazo de generación inconsistente contra Qdrant real. Sólo IA se reemplaza por clientes controlados.

Las colecciones usan prefijo aleatorio `chunking_test_<guid>_`; el test elimina únicamente lo que creó y su carpeta temporal. No borra las colecciones de demostración. Usa `QDRANT_ENDPOINT` del proceso o localhost:6334, sin cargar `.env` ni credenciales AI. No prueba fórmulas internas de Qdrant.

## Pruebas reales con proveedor

No se incluyen en los unit tests ni se ejecutan implícitamente. Son los comandos `ingest` de cada estrategia, `compare` y la misma `query` en las cuatro variantes. Consumen cuota y pueden variar entre ejecuciones. La validación de cobertura corre siempre antes de publicar; no depende de una inspección manual.

Revisá los manifiestos: rangos consecutivos para structure/semantic/llm, overlap sólo en fixed, texto original, similitudes, modelos, dimensión y métricas. El dashboard debe mostrar una colección por estrategia y cantidad coherente con `Chunks.Length`. Una segunda ingesta no debe aumentar puntos por duplicación.

## Errores útiles

- CLI/configuración inválida: mensaje y código 1, sin imprimir claves.
- Qdrant inaccesible: comprobar Docker, puerto gRPC y colección.
- HTTP 400: modelos/endpoints y soporte de JSON Schema.
- HTTP 429: logging de espera/reintento acotado; si persiste, error y reejecución posterior con cuota disponible.
- Propuesta LLM inválida: aborta preparación, sin fallback ni texto reparado.
- Manifiesto perdido o generación incompatible: revisar historial y reingestar/limpiar sólo la colección propia.
- Ctrl+C: cancelación propagada y salida 130.

La última evidencia de ejecución está en [validación observada](09-validacion-observada.md). No se afirma compatibilidad de proveedores no ejecutados.

Fuentes: [xUnit v3](https://xunit.net/docs/getting-started/v3/getting-started), [dotnet test y Microsoft Testing Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test).

[README](../README.md) · [Entorno](08-entorno-y-ejecucion.md).
