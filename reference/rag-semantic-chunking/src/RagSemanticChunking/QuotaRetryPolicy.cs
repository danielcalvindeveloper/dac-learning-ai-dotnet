using System.ClientModel.Primitives;
using Microsoft.Extensions.Logging;

namespace RagSemanticChunking;

// Extendemos el mecanismo del SDK: no creamos un scheduler ni reintentamos propuestas inválidas.
/// <summary>Ajusta la espera del retry del SDK para cuotas temporales, con un máximo de dos reintentos.</summary>
/// <remarks>No reintenta propuestas de límites inválidas ni reemplaza la política de transporte del SDK.</remarks>
public sealed class QuotaRetryPolicy(ILogger logger) : ClientRetryPolicy(maxRetries: 2)
{
    /// <summary>Conserva la política base y exige al menos un minuto de espera ante HTTP 429.</summary>
    /// <remarks>El SDK ejecuta la espera y respeta cancelación. Las métricas cuentan decisiones, no intentos HTTP.</remarks>
    protected override TimeSpan GetNextDelay(PipelineMessage message, int tryCount)
    {
        TimeSpan delay = base.GetNextDelay(message, tryCount);
        if (message.Response?.Status == 429)
        {
            // Una ventana completa evita agotar enseguida reintentos ante cuotas por minuto.
            // Si Retry-After pide más, conservamos el tiempo calculado por el SDK.
            if (delay < TimeSpan.FromSeconds(60)) delay = TimeSpan.FromSeconds(60);
            logger.LogWarning("Cuota AI temporal: reintento {Attempt}/2 en {Seconds:F0} segundos", tryCount, delay.TotalSeconds);
        }
        return delay;
    }
}
