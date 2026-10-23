# Entorno, ejecución y limpieza

## Desde cero

Instalá [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) y [Docker Desktop](https://docs.docker.com/desktop/) con contenedores Linux. Se necesita un proveedor con embeddings y chat; la estrategia LLM requiere JSON Schema. No se requiere Lucene, MAF, MCP ni un servidor .NET.

```powershell
dotnet --version
docker --version
docker compose version
docker info
cd reference/rag-semantic-chunking
docker compose up -d
docker compose ps
curl.exe http://localhost:6333/healthz
dotnet restore
dotnet build
dotnet test
```

Todos los comandos siguientes parten de esta carpeta. Docker debe estar iniciado antes de Compose. [docker-compose.yml](../docker-compose.yml) conserva el proyecto `dac-rag-vector-store`, imagen **qdrant/qdrant:v1.19.2**, volumen `qdrant-data` y puertos loopback **6333 REST / 6334 gRPC**. Comparte infraestructura con las otras dos referencias, pero usa cuatro colecciones nuevas. No crea un segundo Qdrant que compita por los puertos. [Instalación oficial](https://qdrant.tech/documentation/installation/).

## Configuración raíz

Prepará el `.env` de la raíz según [.env.example](../../../.env.example). No crees un `.env` local que oculte esa configuración. [Ejemplo local](../.env.example) sólo recuerda el endpoint Qdrant existente.

```env
AI_PROVIDER=openai
AI_MODEL=gpt-4o-mini
AI_EMBEDDING_MODEL=text-embedding-3-small
AI_API_KEY=tu-api-key
AI_URL=
QDRANT_ENDPOINT=http://localhost:6334
```

`AI_PROVIDER`, ambos modelos y clave son requeridos. `AI_MODEL` genera respuestas y elige límites LLM; `AI_EMBEDDING_MODEL` produce los vectores de indexación, consultas y unidades semánticas. Son capacidades diferentes. El ejemplo conserva modelos documentados previamente; no garantiza capacidades de cualquier modelo compatible con chat.

`AI_URL` admite endpoint HTTP/HTTPS compatible opcional; vacío usa el endpoint del SDK. `QDRANT_ENDPOINT` tiene default explícito localhost:6334 y requiere host/puerto gRPC sin ruta/query. Las URLs no admiten credenciales incrustadas. No hay nuevas variables.

`AppConfiguration` usa `Env.TraversePath().Load()`, valida y no registra secretos. Los clientes reciben su configuración por constructor. `QuotaRetryPolicy` extiende la política SDK con hasta dos reintentos y espera ante 429; puede haber pausas de un minuto o más según Retry-After, cancelables con Ctrl+C. No es un mecanismo para evitar cuotas: un límite persistente seguirá fallando.

## Ingestar las cuatro variantes

```powershell
dotnet run --project src/RagSemanticChunking -- ingest --chunking fixed
dotnet run --project src/RagSemanticChunking -- ingest --chunking structure
dotnet run --project src/RagSemanticChunking -- ingest --chunking semantic
dotnet run --project src/RagSemanticChunking -- ingest --chunking llm
```

Cada comando prepara el mismo corpus completo, valida y publica su colección/manifiesto. `semantic` consume embeddings extra; `llm` consume múltiples decisiones de chat. El modelo puede generar límites distintos en una segunda ejecución.

`data/` y `examples/queries.json` se copian junto al ejecutable. `artifacts/` se resuelve desde el directorio de trabajo y está ignorada por Git. Para editar/retirar documentos, reconstruí y comprobá que no queden copias antiguas en output. No hay carga de PDF/Word/OCR ni watcher.

## Consultar y comparar

```powershell
dotnet run --project src/RagSemanticChunking -- query "¿Qué ticket uso si pierdo el teléfono de MFA?" --chunking fixed
dotnet run --project src/RagSemanticChunking -- query "¿Qué ticket uso si pierdo el teléfono de MFA?" --chunking structure
dotnet run --project src/RagSemanticChunking -- query "¿Qué ticket uso si pierdo el teléfono de MFA?" --chunking semantic
dotnet run --project src/RagSemanticChunking -- query "¿Qué ticket uso si pierdo el teléfono de MFA?" --chunking llm
dotnet run --project src/RagSemanticChunking -- compare
```

`query` muestra hits y completa RAG; `--search-only` evita chat. `compare` ejecuta diez preguntas sobre las cuatro colecciones, comparte un vector por pregunta y guarda `artifacts/comparison.md` y `.json`. No genera respuestas ni reingesta. Requiere manifiestos comparables y corpus sin cambios. Los vectores completos nunca se imprimen.

Dashboard: http://localhost:6333/dashboard. Inspeccioná `dac_chunking_fixed`, `dac_chunking_structure`, `dac_chunking_semantic` y `dac_chunking_llm`. La dimensión se obtiene del embedding real; no está fijada a 1536/3072 en el código.

## Tests y cleanup

```powershell
dotnet test --project tests/RagSemanticChunking.IntegrationTests
```

Cerrar la consola conserva puntos y manifiestos. `docker compose down` detiene Qdrant **también para las otras referencias**, manteniendo volumen. `up -d` lo recupera.

Para cambiar proveedor/modelo o reiniciar el experimento: detener ingestas/consultas, borrar en dashboard **sólo las cuatro colecciones `dac_chunking_*`**, y retirar sus cuatro `*-manifest.json` de esta referencia. Comprobar las rutas antes de borrarlas y conservar los otros references. Luego ejecutar las cuatro ingestas.

No usar `docker compose down --volumes` como limpieza rutinaria: borra datos Qdrant compartidos. Los reportes de `artifacts/` son prescindibles; los manifiestos permiten comprobar identidad y retirar IDs antiguos. No se elimina nada automáticamente al iniciar.

Qdrant local no tiene autenticación y está limitado a loopback. Producción requiere controles propios. [Seguridad Qdrant](https://qdrant.tech/documentation/tutorials-operations/secure-qdrant/), [volúmenes Docker](https://docs.docker.com/engine/storage/volumes/).

[README](../README.md) · [Testing](07-testing.md).
