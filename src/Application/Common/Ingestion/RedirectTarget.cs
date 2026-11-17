using Application.Common.Interfaces;

namespace Application.Common.Ingestion;

// Por qué no se sigue un salto de redirección. Viaja como dato y no como texto comparado, para que el motivo del
// rechazo y el mensaje que se registra sean el mismo sin que nadie tenga que interpretar una frase.
public enum RedirectRefusal
{
    None = 0,
    NoDestination = 1,
    NotHttps = 2,
    OutsideStore = 3,
    TooManyHops = 4,
    NotASheet = 5
}

// Resultado de la decisión: o un destino concreto o el motivo del rechazo, nunca las dos cosas.
public sealed record RedirectDecision(string? Target, RedirectRefusal Refusal)
{
    public bool Allowed => Target is not null;

    public static RedirectDecision Follow(string target) => new(target, RedirectRefusal.None);

    public static RedirectDecision Refuse(RedirectRefusal refusal) => new(null, refusal);
}

/// <summary>
/// Decisión pura sobre un salto de redirección: si se puede seguir y con qué URL. Vive en la aplicación, sin red ni
/// estado, porque el descubrimiento y los dos conectores tienen que responder lo mismo y la decisión tiene que poder
/// comprobarse sin salir de la red.
/// </summary>
public static class RedirectTarget
{
    /// <summary>
    /// Saltos que se siguen por lectura. Dos cubren los casos medidos en las tiendas chilenas —<c>sitemap.xml</c> que
    /// responde 301 a <c>sitemap_index.xml</c>, y el host con y sin "www"— y acotan el coste: cada salto es una
    /// petición más, pasa por el limitador del host y se cuenta contra el presupuesto de la ejecución.
    /// </summary>
    public const int MaximumHops = 2;

    /// <summary>
    /// Decide el próximo salto de una lectura. <paramref name="followed"/> es cuántas redirecciones se siguieron
    /// antes de esta respuesta y <paramref name="isSheetTarget"/> la regla de la plataforma cuando lo que se está
    /// leyendo es una ficha; sin regla (un documento del sitemap) sólo se valida el host.
    /// </summary>
    public static RedirectDecision Resolve(
        StoreContext store, string currentUrl, string? location, int followed,
        Func<string, bool>? isSheetTarget = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        // El tope se comprueba antes que el destino: una tercera redirección no se sigue ni siquiera cuando su
        // destino sería válido, porque una cadena así es justamente lo que el tope existe para detener.
        if (followed >= MaximumHops) return RedirectDecision.Refuse(RedirectRefusal.TooManyHops);

        // Sin destino utilizable no hay lectura que continuar: no se adivina la ruta que la tienda quiso indicar.
        if (string.IsNullOrWhiteSpace(location) ||
            !Uri.TryCreate(currentUrl, UriKind.Absolute, out var current) ||
            !Uri.TryCreate(current, location.Trim(), out var target) || !target.IsAbsoluteUri)
            return RedirectDecision.Refuse(RedirectRefusal.NoDestination);

        if (target.Scheme != Uri.UriSchemeHttps) return RedirectDecision.Refuse(RedirectRefusal.NotHttps);

        // El host se compara normalizado (el prefijo "www." es la misma tienda), pero el puerto y las credenciales no:
        // un destino con puerto propio o con usuario es otra superficie de la red, no la tienda configurada.
        if (!string.Equals(StoreContext.NormalizeHost(target.Host), store.Host, StringComparison.Ordinal) ||
            !target.IsDefaultPort || target.UserInfo.Length != 0)
            return RedirectDecision.Refuse(RedirectRefusal.OutsideStore);

        // El fragmento no viaja en la petición: la URL que se pide y se vuelve a validar es la que tiene destino.
        var targetUrl = target.GetLeftPart(UriPartial.Query);

        // La regla de la plataforma se aplica al destino igual que a la URL original: una redirección no convierte
        // una ficha en otra cosa, ni siquiera dentro del mismo host.
        if (isSheetTarget is not null && !isSheetTarget(targetUrl))
            return RedirectDecision.Refuse(RedirectRefusal.NotASheet);

        return RedirectDecision.Follow(targetUrl);
    }
}
