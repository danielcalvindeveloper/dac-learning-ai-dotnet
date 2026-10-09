public static class DocumentLoader
{
    public static async Task<string> LoadAsync(string documentPath)
    {
        if (!File.Exists(documentPath))
        {
            throw new FileNotFoundException(
                $"No se encontró el documento: {documentPath}", documentPath);
        }

        string extension = Path.GetExtension(documentPath);
        if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Formato no soportado: {extension}. Los formatos soportados son .txt y .md.");
        }

        return await File.ReadAllTextAsync(documentPath);
    }
}
