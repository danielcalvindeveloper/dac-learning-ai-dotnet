using Microsoft.Extensions.AI;

namespace RagVectorStore;

public sealed record RagAnswer(string Text, IReadOnlyList<string> Sources, bool UsedModel);

public sealed class RagService(IChatClient chatClient)
{
    public async Task<RagAnswer> AnswerAsync(
        string question,
        IReadOnlyList<SearchHit> candidates,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        SearchHit[] accepted = candidates
            .Where(hit => double.IsFinite(hit.Score) && hit.Score >= RagSettings.MinimumScore)
            .ToArray();

        if (accepted.Length == 0)
        {
            return new RagAnswer("No se encontró contexto suficientemente relevante para responder.", [], false);
        }

        RagContext context = RagContext.Build(accepted);
        Console.WriteLine($"\n=== CONTEXTO ACEPTADO ({accepted.Length} chunks) ===\n{context.Text}");
        ChatResponse response = await chatClient.GetResponseAsync(context.Prompt(question), cancellationToken: cancellationToken);
        return new RagAnswer(response.Text, context.Sources, true);
    }
}
