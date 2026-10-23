namespace RagSemanticChunking;

/// <summary>Representa un comando validado, separado de la creación de clientes y ejecución de servicios.</summary>
public sealed record CommandLine(string Command, string? Strategy, string Question, bool SearchOnly)
{
    /// <summary>Conjunto cerrado de estrategias implementadas; compare exige este conjunto completo.</summary>
    public static IReadOnlyList<string> Strategies { get; } = Array.AsReadOnly(new[] { "fixed", "structure", "semantic", "llm" });
    /// <summary>Rechaza nombres desconocidos antes de usarlos en rutas o colecciones.</summary>
    public static string ValidateStrategy(string strategy) => Strategies.Contains(strategy, StringComparer.Ordinal)
        ? strategy : throw new ArgumentException("--chunking debe ser fixed, structure, semantic o llm.");

    /// <summary>Interpreta ingest, query y compare sin agregar un framework de CLI.</summary>
    /// <remarks>La pregunta debe llegar como un solo argumento entre comillas; se rechazan opciones duplicadas o ajenas al comando.</remarks>
    public static CommandLine Parse(string[] args)
    {
        const string usage = "Uso: ingest --chunking <estrategia> | query \"pregunta\" --chunking <estrategia> [--search-only] | compare";
        if (args.Length == 0 || args[0] is not ("ingest" or "query" or "compare")) throw new ArgumentException(usage);
        string command = args[0], question = "";
        string? strategy = null;
        bool searchOnly = false;
        int i = 1;
        if (command == "query")
        {
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]) || args[1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException(usage);
            question = args[i++];
        }
        for (; i < args.Length; i++)
        {
            if (args[i] == "--chunking" && strategy is null && i + 1 < args.Length && command != "compare")
                strategy = ValidateStrategy(args[++i]);
            else if (args[i] == "--search-only" && command == "query" && !searchOnly) searchOnly = true;
            else throw new ArgumentException(usage);
        }
        // Compare usa las cuatro estrategias; ingest y query necesitan seleccionar exactamente una.
        if (command != "compare" && strategy is null) throw new ArgumentException(usage);
        return new(command, strategy, question, searchOnly);
    }
}
