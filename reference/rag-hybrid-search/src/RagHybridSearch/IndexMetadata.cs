using System.Globalization;

namespace RagHybridSearch;

// Metadata pequeña del commit, no un snapshot de todo el corpus ni un segundo índice lexical.
public sealed record IndexMetadata(string EmbeddingModel, int Dimensions, string Generation)
{
    public const string Schema = "lucene-codes-v1";

    public Dictionary<string, string> ToCommitData() => new()
    {
        ["schema"] = Schema, ["model"] = EmbeddingModel,
        ["dimensions"] = Dimensions.ToString(CultureInfo.InvariantCulture), ["generation"] = Generation
    };

    public static IndexMetadata FromCommitData(IDictionary<string, string> values)
    {
        if (!values.TryGetValue("schema", out string? schema) || schema != Schema
            || !values.TryGetValue("model", out string? model) || string.IsNullOrWhiteSpace(model)
            || !values.TryGetValue("dimensions", out string? dimensions)
            || !int.TryParse(dimensions, NumberStyles.None, CultureInfo.InvariantCulture, out int count) || count <= 0
            || !values.TryGetValue("generation", out string? generation) || !Guid.TryParse(generation, out _))
            throw new InvalidOperationException("El commit Lucene no corresponde al esquema actual. Revisá el índice y ejecutá ingest.");
        return new(model, count, generation);
    }
}
