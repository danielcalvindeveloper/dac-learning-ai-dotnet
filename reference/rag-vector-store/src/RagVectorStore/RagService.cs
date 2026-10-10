using Microsoft.Extensions.AI;

namespace RagVectorStore;

// UsedModel indica una llamada al chat; la búsqueda previa sí puede haber consumido embeddings.
public sealed record RagAnswer(string Text, IReadOnlyList<string> Sources, bool UsedModel);

public sealed class RagService(IChatClient chatClient)
{
    public async Task<RagAnswer> AnswerAsync(
        string question,
        IReadOnlyList<SearchHit> candidates,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        // Recuperar los mejores disponibles y aceptar evidencia son decisiones separadas.
        SearchHit[] accepted = candidates
            .Where(hit => double.IsFinite(hit.Score) && hit.Score >= RagSettings.MinimumScore)
            .ToArray();

        if (accepted.Length == 0)
        {
            // Salida determinista: sin evidencia aceptada no solicitamos una respuesta generativa.
            return new RagAnswer("No se encontró contexto suficientemente relevante para responder.", [], false);
        }

        RagContext context = RagContext.Build(accepted);
        Console.WriteLine($"\n=== CONTEXTO ACEPTADO ({accepted.Length} chunks) ===\n{context.Text}");
        ChatResponse response = await chatClient.GetResponseAsync(context.Prompt(question), cancellationToken: cancellationToken);
        return new RagAnswer(response.Text, context.Sources, true);
    }
}
