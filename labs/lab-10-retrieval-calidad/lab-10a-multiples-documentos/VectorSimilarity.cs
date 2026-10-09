public static class VectorSimilarity
{
    public static double CosineSimilarity(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length || vectorA.IsEmpty)
        {
            throw new ArgumentException("Los vectores deben tener igual dimensión y no estar vacíos.");
        }

        double dot = 0;
        double magnitudeA = GetMagnitude(vectorA);
        double magnitudeB = GetMagnitude(vectorB);

        for (int i = 0; i < vectorA.Length; i++)
            dot += (double)vectorA[i] * vectorB[i];


        return dot / (magnitudeA * magnitudeB);
    }

    private static double GetMagnitude(ReadOnlySpan<float> vector)
    {

        double squaredMagnitude = 0;

        for (int i = 0; i < vector.Length; i++)
        {
            double a = vector[i];

            if (!double.IsFinite(a))
                throw new ArgumentException("Los vectores deben contener solamente números finitos.");

            squaredMagnitude += a * a;
        }

        double magnitude = Math.Sqrt(squaredMagnitude);

        if (magnitude == 0)
        {
            throw new ArgumentException("La similitud coseno no está definida para vectores de magnitud cero.");
        }

        return magnitude;
    }
}
