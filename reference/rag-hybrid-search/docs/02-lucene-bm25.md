# Lucene.NET y BM25

Lucene.NET mantiene un índice invertido: relaciona términos con los documentos que los contienen. Cada chunk es un documento Lucene. El motor administra análisis, postings, estadísticas, persistencia y ranking; LexicalSearchService adapta sus resultados al modelo común.

## Versión

Lucene.Net y Lucene.Net.Analysis.Common se fijan en **4.8.0-beta00018**, versión vigente consultada de la línea 4.8. Ambos son **prerelease**, no releases estables. La última versión sin sufijo beta listada es 3.0.3, de una línea antigua. Elegimos la línea actual con BM25 y targets modernos y verificamos .NET 10. BM25Similarity también documenta su API como experimental.

Una adopción real debe evaluar soporte y cambios de versión. [NuGet/targets](https://www.nuget.org/packages/Lucene.Net/4.8.0-beta00018), [Analysis.Common](https://www.nuget.org/packages/Lucene.Net.Analysis.Common/4.8.0-beta00018), [proyecto oficial](https://lucenenet.apache.org/).

## BM25 conceptual

TF cuenta ocurrencias en el chunk; DF cuenta chunks que contienen el término; N es cantidad de chunks. IDF favorece términos raros. k1 controla saturación de TF; b, normalización por longitud.

```text
IDF(t) = ln(1 + (N - DF(t) + 0.5) / (DF(t) + 0.5))

aporte(t,d) = IDF(t) * TF(t,d) * (k1 + 1)
             / (TF(t,d) + k1 * (1 - b + b * longitud(d) / longitudMedia))
```

Configuramos BM25Similarity(1.2f, 0.75f) en IndexWriterConfig e IndexSearcher. Lucene calcula los scores; no reproducimos su motor. Las normas de longitud almacenadas tienen precisión reducida y pueden producir empates entre textos diferentes. No exigimos igualdad con esta fórmula simplificada o con la implementación anterior. [BM25Similarity de la versión utilizada](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/core/Lucene.Net.Search.Similarities.BM25Similarity.html).

## Analyzer consciente

CodePreservingAnalyzer compone PatternTokenizer y LowerCaseFilter de Lucene con este patrón:

```text
[\p{L}\p{N}]+(?:-[\p{L}\p{N}]+)*
```

Grupo 0 captura el término completo: ART-93823 pasa a art-93823, MFA a mfa; año conserva el acento. Se descartan delimitadores, sin stemming, stop words ni sinónimos. Un Analyzer que separe guiones puede perder identidad; por eso configuramos componentes existentes, sin otro tokenizer artesanal.

Indexación y consulta usan la misma cadena. Los códigos admitidos usan guion ASCII; barras, puntos y otras formas requieren revisar análisis y reindexar. ART-93823 no equivale a 93823. [Análisis oficial](https://lucenenet.apache.org/docs/4.8.0-beta00018/api/analysis-common/overview.html).

## Índice y consulta

LuceneIndex usa TextField para texto, StringField para ID/origen/etiquetas, campos almacenados y DocValues para desempatar. FSDirectory persiste en artifacts/lucene-index/.

LexicalSearchService abre DirectoryReader e IndexSearcher. Analiza y deduplica términos; construye TermQuery unidos por SHOULD (basta una coincidencia), sin interpretar QueryParser, frases, comodines u operadores. Admite hasta 64 términos distintos.

Filter no agrega score. El motor devuelve topK con orden BM25 y desempate por fuente/posición. Un score positivo indica coincidencia, no suficiencia para RAG.

Antes había un recorrido lineal propio y un snapshot JSON. Se retiraron: la fórmula queda en docs, el índice invertido y BM25 se delegan a Lucene, independientemente del tamaño del corpus.

[Ingesta](05-ingesta-dual.md) · [RRF](03-hybrid-search-rrf.md).

