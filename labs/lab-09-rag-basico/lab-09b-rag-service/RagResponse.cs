public sealed record RagResponse(
    string Answer,
    IReadOnlyList<SearchResult> Sources);
