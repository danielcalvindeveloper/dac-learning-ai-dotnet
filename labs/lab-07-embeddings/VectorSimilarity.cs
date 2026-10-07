public static class VectorSimilarity
{
    public static double CosineSimilarity(
        ReadOnlySpan<float> vectorA,
        ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length || vectorA.IsEmpty)
        {
            throw new ArgumentException(
                "Los vectores deben tener igual dimensión y no estar vacíos.");
        }

        double dot = 0;
        double squaredMagnitudeA = 0;
        double squaredMagnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            double a = vectorA[i];
            double b = vectorB[i];

            if (!double.IsFinite(a) || !double.IsFinite(b))
            {
                throw new ArgumentException(
                    "Los vectores deben contener solamente números finitos.");
            }

            dot += a * b;
            squaredMagnitudeA += a * a;
            squaredMagnitudeB += b * b;
        }

        double magnitudeA = Math.Sqrt(squaredMagnitudeA);
        double magnitudeB = Math.Sqrt(squaredMagnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            throw new ArgumentException(
                "La similitud coseno no está definida para vectores de magnitud cero.");
        }

        return dot / (magnitudeA * magnitudeB);
    }
}
