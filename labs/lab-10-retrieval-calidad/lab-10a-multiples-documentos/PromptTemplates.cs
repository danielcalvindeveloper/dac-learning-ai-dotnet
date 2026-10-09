public static class PromptTemplates
{
    public static string Rag(string context, string question)
    {
        return $"""
            Respondé utilizando únicamente el contexto proporcionado.
            Si la respuesta no está en el contexto, indicá que no disponés de información suficiente.

            Contexto:
            {context}

            Pregunta:
            {question}
            """;
    }
}
