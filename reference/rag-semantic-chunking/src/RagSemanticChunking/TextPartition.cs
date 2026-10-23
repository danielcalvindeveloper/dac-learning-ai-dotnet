namespace RagSemanticChunking;

/// <summary>Comparte el corte por caracteres y las comprobaciones de integridad entre estrategias.</summary>
public static class TextPartition
{
    /// <summary>Divide un rango buscando párrafo, oración y, como último recurso, límite de tamaño.</summary>
    /// <param name="text">Texto normalizado del documento.</param>
    /// <param name="start">Inicio incluido del rango que se quiere dividir.</param>
    /// <param name="end">Final excluido del rango.</param>
    /// <param name="maxSize">Máximo de unidades UTF-16 por fragmento; no es un límite de tokens.</param>
    /// <param name="overlap">Caracteres que se intenta repetir entre fragmentos consecutivos.</param>
    /// <remarks>La prioridad de separadores es recursiva en concepto; se implementa con un bucle.</remarks>
    public static IReadOnlyList<TextRange> Recursive(string text, int start, int end, int maxSize, int overlap)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSize);
        if (overlap < 0 || overlap >= maxSize) throw new ArgumentOutOfRangeException(nameof(overlap));
        if (start < 0 || end < start || end > text.Length) throw new ArgumentException("Rango inválido.");
        List<TextRange> ranges = [];
        while (start < end)
        {
            int limit = Math.Min(start + maxSize, end);
            int cut = limit;
            string reason = limit == end ? "document/section end" : "max size";
            // Sólo retrocedemos hasta la mitad de la ventana: evita fragmentos diminutos y garantiza avance.
            if (limit < end)
            {
                int lower = start + Math.Max(overlap + 1, maxSize / 2);
                for (int p = limit; p >= lower; p--)
                    if (p >= 2 && text[p - 1] == '\n' && text[p - 2] == '\n')
                    { cut = p; reason = "paragraph"; break; }
                // Sin un párrafo disponible probamos un final de oración; si tampoco existe,
                // conservamos el límite duro. Esta estrategia no interpreta el significado del texto.
                if (cut == limit)
                    for (int p = limit; p >= lower; p--)
                        if (p >= 2 && char.IsWhiteSpace(text[p - 1]) && text[p - 2] is '.' or '!' or '?')
                        { cut = p; reason = "sentence"; break; }
                cut = SafeCut(text, cut);
                if (cut <= start) throw new ArgumentException("El tamaño no permite conservar el carácter Unicode completo.");
            }
            ranges.Add(new(start, cut, reason));
            // Al llegar al final no generamos otra ventana: repetir sólo el overlap sería redundante.
            if (cut == end) break;
            int next = SafeCut(text, cut - overlap);
            // Una ventana diminuta puede consumir todo el avance al conservar Unicode/overlap.
            // En ese caso reducimos overlap antes que repetir el mismo inicio indefinidamente.
            start = next > start ? next : cut;
        }
        return ranges;
    }

    /// <summary>Evita partir un par sustituto UTF-16, como el de muchos emojis, retrocediendo una posición.</summary>
    /// <remarks>No preserva necesariamente un grafema formado por varios caracteres combinados.</remarks>
    public static int SafeCut(string text, int position) => position > 0 && position < text.Length
        && char.IsHighSurrogate(text[position - 1]) && char.IsLowSurrogate(text[position]) ? position - 1 : position;

    /// <summary>Rechaza pérdidas, reescrituras, rangos inválidos y chunks que no avanzan la cobertura.</summary>
    /// <param name="document">Texto de referencia contra el que se comprueba cada slice.</param>
    /// <param name="chunks">Fragmentos en orden de lectura.</param>
    /// <param name="maxSize">Tamaño máximo permitido para cada fragmento.</param>
    /// <param name="overlap">Si es false, cada inicio debe coincidir con el final anterior.</param>
    public static void ValidateCoverage(DocumentText document, IReadOnlyList<DocumentChunk> chunks, int maxSize, bool overlap)
    {
        // covered indica hasta dónde llegó la cobertura continua, no la suma de tamaños.
        // Sumar tamaños contaría dos veces el overlap y podría ocultar huecos.
        int covered = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            DocumentChunk c = chunks[i];
            if (c.Index != i || c.Source != document.Source || c.Start < 0 || c.End > document.Text.Length
                || c.End <= c.Start || c.End - c.Start > maxSize || c.Start > covered
                || (i == 0 && c.Start != 0) || (!overlap && c.Start != covered)
                || c.End <= covered || c.Content != document.Text[c.Start..c.End])
                throw new InvalidOperationException("Chunks inválidos: pérdida, reescritura, duplicación o rangos fuera de límites.");
            covered = c.End;
        }
        if (covered != document.Text.Length) throw new InvalidOperationException("Los chunks no cubren todo el documento.");
    }
}
