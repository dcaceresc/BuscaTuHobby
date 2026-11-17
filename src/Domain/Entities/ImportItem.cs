namespace Domain.Entities;

// Estado de una entrada del catálogo dentro de una ejecución. Los números se guardan en la columna y son el
// contrato de la consulta de reanudación: pendiente y el error transitorio son lo único que se vuelve a procesar.
public enum ImportItemState
{
    Pending = 0,
    Stored = 1,
    Skipped = 2,
    Failed = 3
}

// Una URL del catálogo descubierta en el sitemap, con lo que se sabe de su intento. Sin esta fila, depurar una
// ingesta es adivinar: aquí quedan la URL, el código HTTP, el estado y el error.
public class ImportItem
{
    public const int MaximumSourceUrlLength = 2048;
    public const int MaximumErrorMessageLength = 512;

    private ImportItem(Guid importRunId, string sourceUrl, ImportItemState state, string? errorMessage)
    {
        ImportItemId = Guid.NewGuid();
        ImportRunId = importRunId;
        SourceUrl = sourceUrl;
        State = state;
        ErrorMessage = errorMessage;
    }

    public Guid ImportItemId { get; private set; }
    public Guid ImportRunId { get; private set; }

    // La URL se guarda tal como la publicó el sitemap: normalizarla inventaría una identidad que la tienda no
    // declaró y haría que el índice único de la ejecución comparara cosas distintas.
    public string SourceUrl { get; private set; } = default!;
    public ImportItemState State { get; private set; }

    // Sólo existe cuando hubo una petición: un descarte por robots.txt no tiene código HTTP.
    public int? HttpStatus { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? AttemptedAtUtc { get; private set; }

    public virtual ImportRun ImportRun { get; private set; } = default!;

    // Un pendiente es trabajo por hacer; un error transitorio también, porque la petición no llegó a responder.
    public bool IsTransientFailure => State == ImportItemState.Failed &&
        (HttpStatus is null or 429 or >= 500);

    public bool IsProcessable => State == ImportItemState.Pending || IsTransientFailure;

    public static ImportItem Create(Guid importRunId, string sourceUrl)
    {
        ValidateRun(importRunId);
        ValidateSourceUrl(sourceUrl);
        return new ImportItem(importRunId, sourceUrl, ImportItemState.Pending, null);
    }

    // Un descarte sin petición: robots.txt excluyó la ruta o una ejecución anterior ya la guardó. No hay código
    // HTTP ni fecha de intento porque no se consultó nada.
    public static ImportItem CreateSkipped(Guid importRunId, string sourceUrl, string errorMessage)
    {
        ValidateRun(importRunId);
        ValidateSourceUrl(sourceUrl);
        ValidateErrorMessage(errorMessage);
        return new ImportItem(importRunId, sourceUrl, ImportItemState.Skipped, errorMessage);
    }

    public void MarkStored(int httpStatus, DateTime attemptedAtUtc)
    {
        ValidateHttpStatus(httpStatus);
        ValidateAttempt(attemptedAtUtc);
        State = ImportItemState.Stored;
        HttpStatus = httpStatus;
        ErrorMessage = null;
        AttemptedAtUtc = attemptedAtUtc;
    }

    // El código es opcional porque un timeout no tiene respuesta que citar; ese caso sigue siendo transitorio.
    public void MarkFailed(int? httpStatus, string errorMessage, DateTime attemptedAtUtc)
    {
        if (httpStatus is not null) ValidateHttpStatus(httpStatus.Value);
        ValidateErrorMessage(errorMessage);
        ValidateAttempt(attemptedAtUtc);
        State = ImportItemState.Failed;
        HttpStatus = httpStatus;
        ErrorMessage = errorMessage;
        AttemptedAtUtc = attemptedAtUtc;
    }

    public void MarkSkipped(int? httpStatus, string errorMessage, DateTime attemptedAtUtc)
    {
        if (httpStatus is not null) ValidateHttpStatus(httpStatus.Value);
        ValidateErrorMessage(errorMessage);
        ValidateAttempt(attemptedAtUtc);
        State = ImportItemState.Skipped;
        HttpStatus = httpStatus;
        ErrorMessage = errorMessage;
        AttemptedAtUtc = attemptedAtUtc;
    }

    public static bool IsValidSourceUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url) && url.Length <= MaximumSourceUrlLength && url == url.Trim() &&
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps &&
        uri.Host.Length != 0 && uri.UserInfo.Length == 0;

    private static void ValidateRun(Guid importRunId)
    {
        if (importRunId == Guid.Empty)
            throw new ArgumentException("La ejecución del elemento es obligatoria.", nameof(importRunId));
    }

    private static void ValidateSourceUrl(string? url)
    {
        if (!IsValidSourceUrl(url))
            throw new ArgumentException("La URL de origen del elemento no es válida.", nameof(url));
    }

    private static void ValidateErrorMessage(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage) || errorMessage.Length > MaximumErrorMessageLength ||
            errorMessage != errorMessage.Trim())
            throw new ArgumentException("El mensaje de error del elemento no es válido.", nameof(errorMessage));
    }

    private static void ValidateHttpStatus(int httpStatus)
    {
        if (httpStatus is < 100 or > 599)
            throw new ArgumentException("El código HTTP del elemento no es válido.", nameof(httpStatus));
    }

    private static void ValidateAttempt(DateTime attemptedAtUtc)
    {
        if (attemptedAtUtc == default || attemptedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("La fecha del intento debe ser una fecha UTC.", nameof(attemptedAtUtc));
    }
}
