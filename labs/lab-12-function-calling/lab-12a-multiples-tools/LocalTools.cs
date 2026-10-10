internal static class LocalTools
{
    public static decimal CalcularTotalConIva(decimal importe, decimal porcentaje)
    {
        Console.WriteLine("=== TOOL EJECUTADA ===");
        Console.WriteLine("Tool: CalcularTotalConIva");
        Console.WriteLine($"importe: {importe}");
        Console.WriteLine($"porcentaje: {porcentaje}");

        ArgumentOutOfRangeException.ThrowIfNegative(importe);
        ArgumentOutOfRangeException.ThrowIfNegative(porcentaje);

        decimal total = importe + importe * porcentaje / 100m;
        Console.WriteLine($"resultado: {total}");
        return total;
    }

    public static decimal CalcularDescuento(decimal importe, decimal porcentaje)
    {
        Console.WriteLine("=== TOOL EJECUTADA ===");
        Console.WriteLine("Tool: CalcularDescuento");
        Console.WriteLine($"importe: {importe}");
        Console.WriteLine($"porcentaje: {porcentaje}");

        ArgumentOutOfRangeException.ThrowIfNegative(importe);

        if (porcentaje < 0 || porcentaje > 100)
        {
            Console.WriteLine("ERROR: El porcentaje de descuento debe estar entre 0 y 100.");
            throw new ArgumentOutOfRangeException(
                nameof(porcentaje),
                "El porcentaje de descuento debe estar entre 0 y 100.");
        }

        decimal total = importe - importe * porcentaje / 100m;
        Console.WriteLine($"resultado: {total}");
        return total;
    }

    public static double ConvertirCelsiusAFahrenheit(double celsius)
    {
        Console.WriteLine("=== TOOL EJECUTADA ===");
        Console.WriteLine("Tool: ConvertirCelsiusAFahrenheit");
        Console.WriteLine($"celsius: {celsius}");

        double fahrenheit = celsius * 9 / 5 + 32;
        Console.WriteLine($"resultado: {fahrenheit}");
        return fahrenheit;
    }
}
