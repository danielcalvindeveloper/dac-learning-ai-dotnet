public static class PromptTemplates
{
    public static string Traducir(
        string idioma,
        string texto)
    {
        return $$"""
        Traduce al {{idioma}}.

        Texto:
        {{texto}}
        """;
    }

    public static string Resumir(
        int lineas,
        string texto)
    {
        return $$"""
        Resume el siguiente texto en un máximo de {{lineas}} líneas:

        {{texto}}
        """;
    }

    public static string Consultar(
        string rol,
        string pregunta)
    {
        return $$"""
        Actúa como un {{rol}}.

        Pregunta:

        {{pregunta}}
        """;
    }
}
