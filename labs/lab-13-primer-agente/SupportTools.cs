using System.Text.Json;

internal sealed class SupportTools
{
    private readonly Dictionary<string, ServiceStatus> serviceStatuses;
    private readonly Dictionary<string, string> knowledgeBase;
    private readonly List<string> incidents = [];

    public SupportTools(string dataDirectory)
    {
        serviceStatuses = new(
            JsonSerializer.Deserialize<Dictionary<string, ServiceStatus>>(
                File.ReadAllText(Path.Combine(dataDirectory, "service-status.json")),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("El estado de servicios no contiene datos."),
            StringComparer.OrdinalIgnoreCase);

        knowledgeBase = new(
            JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(Path.Combine(dataDirectory, "knowledge-base.json")))
                ?? throw new InvalidOperationException("La base de conocimiento no contiene datos."),
            StringComparer.OrdinalIgnoreCase);
    }

    public string ConsultarEstadoServicio(string servicio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(servicio);

        if (!serviceStatuses.TryGetValue(servicio.Trim(), out ServiceStatus? status))
        {
            return $"No hay información del servicio {servicio}.";
        }

        return $"El servicio {servicio.Trim()} está {status.Status}." +
            (string.IsNullOrWhiteSpace(status.Incident)
                ? " No hay un incidente conocido."
                : $" Incidente conocido: {status.Incident}.");
    }

    public string BuscarSolucion(string concepto)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concepto);

        return knowledgeBase.TryGetValue(concepto.Trim(), out string? solution)
            ? $"Solución conocida para {concepto.Trim()}: {solution}"
            : $"No existe una solución conocida para {concepto.Trim()}.";
    }

    public string RegistrarIncidente(string servicio, string descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(servicio);
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        string id = $"INC-{1001 + incidents.Count}";
        incidents.Add($"{id} | {servicio.Trim()} | {descripcion.Trim()}");

        return $"Se registró el incidente {id} para {servicio.Trim()}: " +
            $"{descripcion.Trim()}. El registro es local y sólo vive en esta ejecución.";
    }

    private sealed record ServiceStatus(string Status, string? Incident);
}
