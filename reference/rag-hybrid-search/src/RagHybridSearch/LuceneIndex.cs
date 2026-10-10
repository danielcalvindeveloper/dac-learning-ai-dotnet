using System.Globalization;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace RagHybridSearch;

public static class LuceneIndex
{
    public const string Text = "text", Id = "id", Source = "source", ChunkIndex = "chunkIndex";
    public const string Department = "department", DocumentType = "documentType", Year = "year";
    public const string SourceSort = "sourceSort", ChunkSort = "chunkSort";

    public static bool Exists(string path)
    {
        if (!System.IO.Directory.Exists(path)) return false;
        using FSDirectory directory = FSDirectory.Open(new DirectoryInfo(path));
        return DirectoryReader.IndexExists(directory);
    }

    public static void Rebuild(string path, IReadOnlyList<DocumentRecord> records, IndexMetadata metadata, CancellationToken token)
    {
        IndexMetadata.FromCommitData(metadata.ToCommitData());
        if (records.Count == 0 || records.Select(record => record.Id).Distinct().Count() != records.Count
            || records.Any(record => record.Generation != metadata.Generation))
            throw new ArgumentException("La ingesta requiere chunks únicos de una misma generación.", nameof(records));
        using FSDirectory directory = FSDirectory.Open(new DirectoryInfo(path));
        using CodePreservingAnalyzer analyzer = new();
        IndexWriterConfig config = new(LuceneVersion.LUCENE_48, analyzer)
        {
            OpenMode = OpenMode.CREATE,
            Similarity = new BM25Similarity((float)SearchSettings.Bm25K1, (float)SearchSettings.Bm25B)
        };
        using IndexWriter writer = new(directory, config);
        try
        {
            // CREATE reemplaza el corpus lexical en el nuevo commit: también retira chunks que ya no existen.
            // UpdateDocument expresa la identidad compartida y evita duplicar un ID.
            foreach (DocumentRecord record in records)
            {
                token.ThrowIfCancellationRequested();
                writer.UpdateDocument(new Term(Id, record.Id.ToString("D")), ToDocument(record));
            }
            writer.SetCommitData(metadata.ToCommitData());
            token.ThrowIfCancellationRequested();
            writer.Commit();
        }
        catch
        {
            // Dispose normalmente confirma cambios: ante una falla queremos conservar el último commit válido.
            writer.Rollback();
            throw;
        }
    }

    private static Document ToDocument(DocumentRecord record) => new()
    {
        new StringField(Id, record.Id.ToString("D"), Field.Store.YES),
        new StringField(Source, record.Source, Field.Store.YES),
        new StoredField(ChunkIndex, record.ChunkIndex.ToString(CultureInfo.InvariantCulture)),
        new TextField(Text, record.Text, Field.Store.YES),
        new StringField(Department, record.Department, Field.Store.YES),
        new StringField(DocumentType, record.DocumentType, Field.Store.YES),
        new StringField(Year, record.Year.ToString(CultureInfo.InvariantCulture), Field.Store.YES),
        new SortedDocValuesField(SourceSort, new BytesRef(record.Source)),
        new NumericDocValuesField(ChunkSort, record.ChunkIndex)
    };

    public static DocumentRecord ToRecord(Document document, string generation) => new()
    {
        Id = Guid.Parse(document.Get(Id)), Source = document.Get(Source), Text = document.Get(Text),
        ChunkIndex = int.Parse(document.Get(ChunkIndex), CultureInfo.InvariantCulture),
        Department = document.Get(Department), DocumentType = document.Get(DocumentType),
        Year = int.Parse(document.Get(Year), CultureInfo.InvariantCulture), Generation = generation
    };
}
