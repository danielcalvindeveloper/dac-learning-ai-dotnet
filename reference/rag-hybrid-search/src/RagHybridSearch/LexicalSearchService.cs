using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace RagHybridSearch;

public sealed record SearchHit(DocumentRecord Record, double Score);

public sealed class LexicalSearchService : IDisposable
{
    private readonly FSDirectory directory;
    private readonly DirectoryReader reader;
    private readonly IndexSearcher searcher;
    private readonly CodePreservingAnalyzer analyzer = new();
    public IndexMetadata Metadata { get; }
    public int Count => reader.NumDocs;

    public LexicalSearchService(string path)
    {
        if (!System.IO.Directory.Exists(path)) throw new InvalidOperationException("No existe el índice Lucene. Ejecutá ingest.");
        directory = FSDirectory.Open(new DirectoryInfo(path));
        DirectoryReader? opened = null;
        try
        {
            if (!DirectoryReader.IndexExists(directory)) throw new InvalidOperationException("No hay un commit Lucene. Ejecutá ingest.");
            opened = DirectoryReader.Open(directory);
            Metadata = IndexMetadata.FromCommitData(opened.IndexCommit.UserData);
            reader = opened;
            searcher = new(reader) { Similarity = new BM25Similarity((float)SearchSettings.Bm25K1, (float)SearchSettings.Bm25B) };
        }
        catch { opened?.Dispose(); directory.Dispose(); analyzer.Dispose(); throw; }
    }

    public IReadOnlyList<SearchHit> Search(string query, MetadataFilter filter, int topK, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);
        token.ThrowIfCancellationRequested();
        string[] terms = Analyze(query);
        if (terms.Length == 0) return [];
        if (terms.Length > SearchSettings.MaximumQueryTerms)
            throw new ArgumentException($"La consulta admite hasta {SearchSettings.MaximumQueryTerms} términos distintos.", nameof(query));
        // TermQuery toma términos literales analizados, no sintaxis de QueryParser ni operadores del usuario.
        BooleanQuery lexical = new();
        foreach (string term in terms) lexical.Add(new TermQuery(new Term(LuceneIndex.Text, term)), Occur.SHOULD);
        Sort order = new(SortField.FIELD_SCORE, new SortField(LuceneIndex.SourceSort, SortFieldType.STRING),
            new SortField(LuceneIndex.ChunkSort, SortFieldType.INT64));
        // El Filter excluye candidatos antes del topK y no agrega puntos al score BM25.
        TopFieldDocs results = searcher.Search(lexical, filter.ToLuceneFilter(), topK, order, true, false);
        token.ThrowIfCancellationRequested();
        return results.ScoreDocs.Select(hit => new SearchHit(
            LuceneIndex.ToRecord(searcher.Doc(hit.Doc), Metadata.Generation), hit.Score)).ToArray();
    }

    private string[] Analyze(string text)
    {
        using TokenStream stream = analyzer.GetTokenStream(LuceneIndex.Text, new StringReader(text));
        ICharTermAttribute term = stream.AddAttribute<ICharTermAttribute>();
        HashSet<string> terms = new(StringComparer.Ordinal);
        stream.Reset();
        while (stream.IncrementToken()) terms.Add(term.ToString());
        stream.End();
        return terms.Order(StringComparer.Ordinal).ToArray();
    }

    public Guid[] GetIds()
    {
        // Recorrido sólo durante ingesta, para retirar de Qdrant IDs del commit anterior.
        IBits? live = MultiFields.GetLiveDocs(reader);
        return Enumerable.Range(0, reader.MaxDoc).Where(i => live is null || live.Get(i))
            .Select(i => Guid.Parse(reader.Document(i).Get(LuceneIndex.Id))).ToArray();
    }

    public void Dispose() { reader.Dispose(); directory.Dispose(); analyzer.Dispose(); }
}
