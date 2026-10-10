using System.Globalization;

namespace RagHybridSearch;

public sealed record CommandLine(bool Ingest, string Question, SearchMode Mode, MetadataFilter Filter, bool SearchOnly)
{
    public static CommandLine Parse(string[] args)
    {
        if (args is ["ingest"]) return new(true, "", SearchMode.Hybrid, new(), false);
        if (args.Length == 0 || args[0] != "query") throw new ArgumentException("Usá ingest o query \"pregunta\" --mode vector|bm25|hybrid.");
        string? department = null, type = null;
        int? year = null;
        SearchMode mode = SearchMode.Hybrid;
        bool searchOnly = false;
        HashSet<string> options = [];
        List<string> question = [];
        for (int i = 1; i < args.Length; i++)
        {
            string argument = args[i];
            if (!argument.StartsWith("--", StringComparison.Ordinal)) { question.Add(argument); continue; }
            if (!options.Add(argument)) throw new ArgumentException($"Opción repetida: {argument}");
            if (argument == "--search-only") { searchOnly = true; continue; }
            if (++i >= args.Length || string.IsNullOrWhiteSpace(args[i]) || args[i].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"Falta valor para {argument}.");
            string value = args[i].Trim();
            switch (argument)
            {
                case "--mode":
                    mode = value switch { "vector" => SearchMode.Vector, "bm25" => SearchMode.Bm25, "hybrid" => SearchMode.Hybrid,
                        _ => throw new ArgumentException("Mode debe ser vector, bm25 o hybrid.") };
                    break;
                case "--department": department = value; break;
                case "--type": type = value; break;
                case "--year":
                    if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed) || parsed is < 1 or > 9999)
                        throw new ArgumentException("Year debe ser un año entre 1 y 9999.");
                    year = parsed;
                    break;
                default: throw new ArgumentException($"Opción desconocida: {argument}");
            }
        }
        string text = string.Join(' ', question).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return new(false, text, mode, new MetadataFilter(department, type, year), searchOnly);
    }
}
