public interface IAssistant
{
    Task<string> ChatAsync(string message);

    Task<string> TraducirAsync(
        string idioma,
        string texto);

    Task<string> ResumirAsync(
        int lineas,
        string texto);

    Task<string> ConsultarAsync(
        string rol,
        string pregunta);
}
