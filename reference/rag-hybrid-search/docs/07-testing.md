# Testing y comparación

## Suite sin servicios externos

```powershell
dotnet restore
dotnet build
dotnet test
```

La solución ejecuta **44 pruebas**: unit tests de lógica propia y seis de integración local Lucene en directorios temporales. Los tests no requieren Docker, red ni credenciales.

| Archivo | Verificación |
|---|---|
| [SearchTests.cs](../tests/RagHybridSearch.Tests/SearchTests.cs) | RRF, deduplicación, empates, aceptación y expresión Qdrant |
| [ConfigurationAndIndexTests.cs](../tests/RagHybridSearch.Tests/ConfigurationAndIndexTests.cs) | CLI, manifest, IDs, dimensión y metadata de commit |
| [LuceneIndexTests.cs](../tests/RagHybridSearch.Tests/LuceneIndexTests.cs) | Analyzer, persistencia, campos, actualización/retiro, filtro antes del topK, consulta literal, límites y rollback |
| [AppConfigurationTests.cs](../tests/RagHybridSearch.Tests/AppConfigurationTests.cs) | Modelos y endpoints |
| [DocumentTextTests.cs](../tests/RagHybridSearch.Tests/DocumentTextTests.cs) | Lectura, normalización y chunking |
| [RagServiceTests.cs](../tests/RagHybridSearch.Tests/RagServiceTests.cs) | Contexto, fuentes y no llamar a chat sin evidencia |

No probamos la fórmula interna de Lucene. Probamos cómo configuramos y usamos su índice. Se retiraron tests matemáticos del BM25 artesanal; RRF sí conserva pruebas de nuestra fórmula.

RecordingChatClient devuelve texto controlado. xUnit usa TestContext.Current.CancellationToken y Microsoft Testing Platform seleccionado por global.json. [Guía oficial](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).

## Integración Qdrant + Lucene

```powershell
docker compose up -d
dotnet test --project tests/RagHybridSearch.IntegrationTests
```

El proyecto queda fuera de la solución principal. [QdrantTests](../tests/RagHybridSearch.IntegrationTests/QdrantTests.cs) usa colección Qdrant aleatoria e índice Lucene temporal. Verifica upsert sin duplicados, reconexión, filtros, hybrid retrieval, deduplicación, fuentes y generaciones distintas.

Ambos motores son reales; sólo el embedding remoto se reemplaza por un vector controlado. No usa LLM. Lee QDRANT_ENDPOINT del proceso (default localhost:6334), sin .env. finally elimina sólo sus recursos.

## Comparación manual

```powershell
./examples/Compare-Queries.ps1
```

[queries.json](../examples/queries.json) define casos A–D y documentos esperados. El script ejecuta los tres modos con search-only y guarda doce salidas en artifacts/. Los IDs de caso no controlan la aplicación. No calcula métricas avanzadas.

La validación manual incluye dos ingestas, consultas en otros procesos, respuestas RAG y camino sin evidencia. Los índices deben conservar 68 chunks sin duplicados.

Lucene cambió scores y empates: el caso C ahora también recupera ambas evidencias con BM25. Se informa ese resultado, sin forzar una mejora. Reabrir verifica persistencia entre ejecuciones; no afirma resistencia a pérdida de disco/volumen ni reinicio del contenedor en esta validación.

[Entorno](08-entorno-y-ejecucion.md) · [Resultados](../README.md#comparación-reproducible).

