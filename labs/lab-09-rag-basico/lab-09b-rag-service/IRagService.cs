public interface IRagService
{
    Task IndexDocumentAsync(string documentPath);

    Task<RagResponse> AskAsync(
        string question,
        int topK = 3);
}
