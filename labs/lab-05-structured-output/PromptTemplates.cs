public static class PromptTemplates
{
    public static string ExtraerPersona(string texto)
    {
        return $$"""
        Extrae los datos de la persona.

        Texto:
        {{texto}}
        """;
    }

    public static string ExtraerPersonas(string texto)
    {
        return $$"""
        Extrae una lista de personas mencionadas en el texto.

        Texto:
        {{texto}}
        """;
    }

    public static string ExtraerProducto(string texto)
    {
        return $$"""
        Extrae los datos del producto.

        Texto:
        {{texto}}
        """;
    }
}
