using System.Linq.Expressions;
using System.Globalization;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace RagHybridSearch;

public sealed record MetadataFilter(string? Department = null, string? DocumentType = null, int? Year = null)
{
    // Una intención de filtro, dos traducciones nativas: expresión para Qdrant y Filter para Lucene.
    // Sólo incluimos comparaciones activas con constantes: el provider no debe evaluar nullables ni métodos propios.
    public Expression<Func<DocumentRecord, bool>> ToExpression()
    {
        ParameterExpression record = Expression.Parameter(typeof(DocumentRecord), "record");
        List<Expression> conditions = [];
        if (Department is not null) Add(nameof(DocumentRecord.Department), Department);
        if (DocumentType is not null) Add(nameof(DocumentRecord.DocumentType), DocumentType);
        if (Year is int year) Add(nameof(DocumentRecord.Year), year);
        Expression body = conditions.Count == 0 ? Expression.Constant(true) : conditions.Aggregate(Expression.AndAlso);
        return Expression.Lambda<Func<DocumentRecord, bool>>(body, record);

        void Add(string property, object value) => conditions.Add(Expression.Equal(
            Expression.Property(record, property), Expression.Constant(value)));
    }

    public Filter? ToLuceneFilter()
    {
        BooleanQuery query = new();
        if (Department is not null) Add(LuceneIndex.Department, Department);
        if (DocumentType is not null) Add(LuceneIndex.DocumentType, DocumentType);
        if (Year is int year) Add(LuceneIndex.Year, year.ToString(CultureInfo.InvariantCulture));
        return query.Clauses.Count == 0 ? null : new QueryWrapperFilter(query);

        // StringField no analiza etiquetas: la igualdad mantiene el mismo casing exacto que Qdrant.
        void Add(string field, string value) => query.Add(new TermQuery(new Term(field, value)), Occur.MUST);
    }
}
