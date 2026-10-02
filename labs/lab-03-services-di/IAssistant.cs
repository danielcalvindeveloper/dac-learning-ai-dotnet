public interface IAssistant
{
    Task<string> ChatAsync(string message);

    Task<string> TraducirAsync(string texto);

    Task<string> ResumirAsync(string texto);
}
