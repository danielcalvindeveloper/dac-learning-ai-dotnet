using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Pattern;
using Lucene.Net.Util;

namespace RagHybridSearch;

public sealed class CodePreservingAnalyzer : Analyzer
{
    private static readonly Regex Terms = new(@"[\p{L}\p{N}]+(?:-[\p{L}\p{N}]+)*", RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        // Grupo 0 conserva el término completo: ART-93823 no se convierte en ART + 93823.
        // Indexación y consulta usan exactamente esta misma cadena de análisis de Lucene.
        PatternTokenizer tokenizer = new(reader, Terms, 0);
        return new TokenStreamComponents(tokenizer, new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenizer));
    }
}
