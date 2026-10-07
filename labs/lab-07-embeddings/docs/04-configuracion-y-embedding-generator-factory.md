# Lectura opcional - Configuración y `EmbeddingGeneratorFactory`

## Continuidad

Reutilizamos el patrón de configuración de los labs anteriores:

```text
.env
  ↓
LabConfiguration
  ↓
EmbeddingGeneratorFactory
  ↓
IEmbeddingGenerator<string, Embedding<float>>
```

## Configuración

### Por qué aparece una segunda variable de modelo

Hasta Lab 06 utilizábamos capacidades generativas/chat. `AI_MODEL` era suficiente para identificar el modelo de esas tareas.

En Lab 07 aparece una capacidad distinta: transformar texto en vectores. Un modelo generativo produce texto; un modelo de embeddings produce vectores. No todos los modelos soportan ambas capacidades, incluso dentro del mismo proveedor.

Por eso incorporamos `AI_EMBEDDING_MODEL` ahora. Es una evolución natural de la configuración: con KISS agregamos la distinción cuando la necesitamos.

| Variable | Significado |
|---|---|
| `AI_PROVIDER` | Proveedor activo |
| `AI_MODEL` | Modelo generativo/chat: texto → respuesta generada |
| `AI_EMBEDDING_MODEL` | Modelo de embeddings: texto → vector |
| `AI_API_KEY` | Credencial del proveedor |
| `AI_URL` | Endpoint alternativo opcional |

Ambos modelos pueden quedar configurados simultáneamente. Aunque compartan proveedor, credencial y endpoint, pueden ser modelos distintos. Configurar un modelo en `AI_MODEL` no demuestra que soporte embeddings.

### Qué carga Lab 07

`LabConfiguration.Load()` utiliza `Env.TraversePath().Load()` para encontrar el `.env` de la raíz desde la carpeta del laboratorio.

Requiere `AI_PROVIDER`, `AI_EMBEDDING_MODEL` y `AI_API_KEY`. `AI_URL` es opcional y se valida como URI absoluta mediante `Uri.TryCreate`.

El record expone `ProviderName`, `ApiKey`, `EmbeddingModel` y `Endpoint`. Ante valores inválidos lanza `InvalidOperationException`. No imprime ni contiene defaults o selección por proveedor.

El ejemplo OpenAI del README utiliza `text-embedding-3-small`; no queda fijado en el código. Lab 07 no lee ni requiere `AI_MODEL` y tampoco lo usa como fallback. Los laboratorios anteriores siguen leyendo `AI_MODEL`.

### Continuidad hacia RAG

En [Lab 09a](../../lab-09-rag-basico/lab-09a-rag-explicito/README.md) y [Lab 09b](../../lab-09-rag-basico/lab-09b-rag-service/README.md) esta separación combina dos tareas:

```text
consulta
  ↓
AI_EMBEDDING_MODEL
  ↓
embedding
  ↓
recuperación semántica

pregunta + contexto recuperado
  ↓
AI_MODEL
  ↓
respuesta generada
```

Lab 07 solo genera y compara vectores. Este flujo explica por qué conservamos ambos modelos configurados. Lab 09a implementa el pipeline RAG explícito y Lab 09b encapsula el mismo mecanismo en `RagService`.

## Factory

`EmbeddingGeneratorFactory.Create(configuration)` recibe configuración validada y construye `OpenAI.Embeddings.EmbeddingClient` con `configuration.EmbeddingModel`.

Sin endpoint alternativo utiliza el endpoint estándar del SDK. Si existe `Endpoint`, lo asigna a `OpenAIClientOptions.Endpoint` y utiliza `ApiKeyCredential`.

Finalmente devuelve `client.AsIEmbeddingGenerator()`. No vuelve a validar la configuración, no calcula similitudes ni conoce los textos del ejercicio.

`AI_PROVIDER` conserva la identificación del proveedor. La factory utiliza la API de OpenAI o un endpoint compatible con embeddings indicado explícitamente en `AI_URL`. No asumimos soporte de embeddings por tener compatibilidad con chat.

## Responsabilidades

- `LabConfiguration`: cargar y validar variables;
- `EmbeddingGeneratorFactory`: construir y adaptar el cliente;
- `VectorSimilarity`: calcular similitud coseno;
- `Program.cs`: ejecutar y mostrar el ejercicio.

Así la infraestructura queda fuera del flujo conceptual principal.
