namespace Lab01.HelloLlm;

internal static class LabConsole
{
    public static void WriteError(string message)
    {
        Console.Error.WriteLine(
            $"Error: {message}");
    }

    public static void WriteRequest(
        LabConfiguration configuration,
        string prompt)
    {
        Console.WriteLine(
            $"Proveedor: {configuration.ProviderName}");

        Console.WriteLine(
            $"Modelo: {configuration.Model}");

        Console.WriteLine(
            $"Prompt: {prompt}");

        Console.WriteLine();
        Console.WriteLine("Respuesta:");
    }

    public static void WriteResponse(string response)
    {
        Console.WriteLine(response);
    }
}
