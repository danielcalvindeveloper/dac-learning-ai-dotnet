internal sealed class AgentState
{
    public required string Objective { get; init; }

    public List<string> Observations { get; } = [];

    public int Iteration { get; set; }

    public bool Completed { get; set; }
}
