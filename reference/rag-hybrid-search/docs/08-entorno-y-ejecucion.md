# Entorno, ejecución y limpieza

Instalá [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) y [Docker Desktop](https://docs.docker.com/desktop/) con contenedores Linux. Verificá dotnet --version, docker --version, docker compose version y docker info.

Lucene.NET se instala con restore: librería local, sin servidor ni contenedor. Sus paquetes 4.8.0-beta00018 son prerelease; [detalle](02-lucene-bm25.md).

Trabajá desde reference/rag-hybrid-search/. artifacts/lucene-index/ depende del directorio actual; data/ se lee desde la copia junto al ejecutable. Reconstruí después de editar corpus y revisá output al retirar archivos: build puede dejar copias antiguas.

## Configuración común

Se carga .env raíz con Env.TraversePath().Load(). No crees otro .env aquí.

```env
AI_PROVIDER=openai
AI_MODEL=gpt-4o-mini
AI_EMBEDDING_MODEL=text-embedding-3-small
AI_API_KEY=tu-api-key
AI_URL=
QDRANT_ENDPOINT=http://localhost:6334
```

Proveedor, ambos modelos y clave son requeridos incluso para BM25 search-only. AI_PROVIDER es informativo; los clientes OpenAI admiten AI_URL compatible opcional. Chat produce texto, embeddings vectores; no hay fallback.

URLs HTTP/HTTPS absolutas sin credenciales incrustadas. Qdrant requiere host/puerto gRPC sin ruta, query o fragmento, con default explícito localhost:6334. No hay variables nuevas.

## Ejecutar

```powershell
docker compose up -d
docker compose ps
curl.exe http://localhost:6333/healthz
dotnet restore
dotnet build
dotnet test
dotnet run --project src/RagHybridSearch -- ingest
dotnet run --project src/RagHybridSearch -- query "ART-93823" --mode vector
dotnet run --project src/RagHybridSearch -- query "ART-93823" --mode bm25
dotnet run --project src/RagHybridSearch -- query "ART-93823" --mode hybrid
dotnet run --project src/RagHybridSearch -- query "ART-93821" --mode hybrid --department RRHH --type policy --year 2026
```

--search-only evita chat. BM25 search-only funciona offline tras ingesta, sin Qdrant. Query abre el commit persistido; no reconstruye índices.

Compose comparte proyecto dac-rag-vector-store, imagen qdrant/qdrant:v1.19.2, volumen qdrant-data y puertos loopback 6333 REST y 6334 gRPC con rag-vector-store. Las colecciones son distintas; no hace falta ingerir la anterior. Dashboard: http://localhost:6333/dashboard. Verificá points_count en dac_hybrid_documents. El corpus actual produce 68 documentos en cada motor. Embeddings/chat consumen conexión y cuota.

## Persistencia y recursos

Cerrar la consola conserva volumen Qdrant y commits Lucene. El reader se reutiliza durante cada comando. Program libera recursos, propaga Ctrl+C y registra etapas a stderr; ranking/respuesta van a stdout. Timeout gRPC: diez segundos por llamada.

Lucene es síncrono: comprobamos cancelación alrededor de search y entre escrituras, sin prometer interrumpir búsquedas a mitad. Errores HTTP/gRPC, configuración, JSON, bloqueo de escritor y corrupción reciben mensajes claros, sin respuestas crudas ni claves. Salidas: 1 error, 130 cancelación, 0 incluye falta de evidencia.

## Migración

Build e ingest crean Lucene. El viejo artifacts/hybrid-index.json ya no se consulta y puede retirarse manualmente. No hay conversión automática. Si retiraste/renombraste corpus antes de migrar o perdiste Lucene, reiniciá ambos índices para limpiar puntos Qdrant sin historial. [Ingesta dual](05-ingesta-dual.md).

## Detener y limpiar por separado

docker compose down detiene el Qdrant compartido conservando volumen; afecta también a rag-vector-store. up -d recupera servicio. Lucene local permanece intacto.

Para empezar desde cero, detené consultas/ingestas:

1. En dashboard eliminá sólo dac_hybrid_documents, conservando dac_rag_documents.
2. Desde esta referencia verificá la ruta y eliminá únicamente el índice local:
   ```powershell
   Resolve-Path -LiteralPath ./artifacts/lucene-index
   Remove-Item -LiteralPath ./artifacts/lucene-index -Recurse
   ```
3. Ejecutá ingest.

No se hace automáticamente. docker compose down --volumes elimina Qdrant de ambas referencias y no borra Lucene; no es el cleanup rutinario. Los .txt de comparación son prescindibles. artifacts/ ya está ignorada por Git; no se versionan segmentos.

Fuentes: [Qdrant instalación](https://qdrant.tech/documentation/installation/), [volúmenes Docker](https://docs.docker.com/engine/storage/volumes/), [logging .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging), [Lucene.NET](https://lucenenet.apache.org/).

[README](../README.md) · [Testing](07-testing.md).

