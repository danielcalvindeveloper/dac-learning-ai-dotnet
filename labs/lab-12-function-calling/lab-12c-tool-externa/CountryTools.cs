using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

internal sealed class CountryTools(HttpClient httpClient)
{
    public async Task<string> ConsultarPais(string nombre)
    {
        Console.WriteLine("=== TOOL EJECUTADA ===");
        Console.WriteLine("Tool: ConsultarPais");
        Console.WriteLine($"nombre: {nombre}");

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error("El nombre del país no puede estar vacío.");
        }

        string url = $"https://countries.dev/name/{Uri.EscapeDataString(nombre.Trim())}?fullText=true";

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            Console.WriteLine($"HTTP: {(int)response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Error($"No se encontró el país '{nombre}'.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Error($"La API de países respondió HTTP {(int)response.StatusCode}.");
            }

            CountryInfo[]? countries =
                await response.Content.ReadFromJsonAsync<CountryInfo[]>();

            if (countries is null || countries.Length != 1)
            {
                return Error("La API no devolvió un único país para ese nombre.");
            }

            CountryInfo country = countries[0];

            if (country is null
                || string.IsNullOrWhiteSpace(country.Name)
                || string.IsNullOrWhiteSpace(country.Capital)
                || string.IsNullOrWhiteSpace(country.Region)
                || country.Population is null or < 0)
            {
                return Error("La respuesta de la API está incompleta.");
            }

            string result =
                $"Nombre: {country.Name}\nCapital: {country.Capital}\n" +
                $"Región: {country.Region}\nPoblación publicada por la API: {country.Population}";

            Console.WriteLine(result);
            return result;
        }
        catch (JsonException)
        {
            return Error("La API devolvió una respuesta JSON inválida.");
        }
        catch (HttpRequestException)
        {
            return Error("No se pudo completar la solicitud HTTP a la API de países.");
        }
        catch (TaskCanceledException)
        {
            return Error("La consulta HTTP a la API de países excedió el tiempo disponible.");
        }
    }

    private static string Error(string message)
    {
        Console.WriteLine($"ERROR: {message}");
        return $"Error: {message}";
    }
}

internal sealed record CountryInfo(
    string? Name,
    string? Capital,
    string? Region,
    long? Population);
