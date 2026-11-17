namespace Application.Common.Ingestion;

/// <summary>
/// Configuración de la ingesta programada. Llega de <c>Ingestion:Enabled</c> e <c>Ingestion:IntervalHours</c> y su
/// valor por defecto es apagada: rastrear el catálogo de un tercero nunca puede empezar sin que alguien lo haya
/// encendido en la configuración del entorno. Un intervalo ilegible vuelve al valor por defecto en lugar de
/// romper el arranque del sitio.
/// </summary>
public sealed record IngestionSettings(bool Enabled, int IntervalHours)
{
    // Claves de configuración, escritas una sola vez para que la composición y la documentación no se separen.
    public const string EnabledKey = "Ingestion:Enabled";
    public const string IntervalHoursKey = "Ingestion:IntervalHours";

    public const bool DefaultEnabled = false;
    public const int DefaultIntervalHours = 6;

    // Un intervalo de cero sería un bucle cerrado contra la tienda y uno de meses no sería automatización.
    public const int MinimumIntervalHours = 1;
    public const int MaximumIntervalHours = 168;

    public TimeSpan Interval => TimeSpan.FromHours(IntervalHours);

    public static IngestionSettings Parse(string? enabled, string? intervalHours)
    {
        // Sólo un "true" explícito enciende la automatización: "sí", vacío o ilegible la dejan apagada.
        var on = bool.TryParse(enabled?.Trim(), out var parsedEnabled) && parsedEnabled;
        var hours = int.TryParse(intervalHours?.Trim(), out var parsedHours)
            ? Math.Clamp(parsedHours, MinimumIntervalHours, MaximumIntervalHours)
            : DefaultIntervalHours;
        return new IngestionSettings(on, hours);
    }
}
