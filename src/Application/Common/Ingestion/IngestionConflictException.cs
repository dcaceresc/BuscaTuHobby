namespace Application.Common.Ingestion;

/// <summary>
/// Conflicto de concurrencia: una tienda no puede tener dos ingestas abiertas a la vez. Tiene tipo propio para que el
/// disparo manual responda con un mensaje en español y la pasada programada pueda omitir la tienda y seguir con las
/// demás, en vez de tratar el conflicto como un fallo de la ingesta.
/// </summary>
public sealed class IngestionConflictException(string message) : Exception(message);
