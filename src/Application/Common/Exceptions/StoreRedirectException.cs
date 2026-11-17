using Application.Common.Ingestion;
using Application.Common.Interfaces;

namespace Application.Common.Exceptions;

/// <summary>
/// Redirección que la ingesta no sigue. El motivo viaja como dato y el mensaje se escribe una sola vez, en español y
/// con el destino concreto, porque este texto queda registrado como motivo de descarte y una persona tiene que poder
/// entender por qué se detuvo la lectura.
/// </summary>
public sealed class StoreRedirectException(string message) : Exception(message)
{
    // Sin destino que mostrar, el motivo de los saltos que sí lo tienen no aplica: cada frase lleva sólo lo que la
    // respuesta realmente informó.
    public static StoreRedirectException For(
        RedirectRefusal refusal, StoreContext store, string? destination)
    {
        ArgumentNullException.ThrowIfNull(store);
        var target = string.IsNullOrWhiteSpace(destination) ? "(sin destino)" : destination.Trim();
        return new StoreRedirectException(refusal switch
        {
            RedirectRefusal.NoDestination =>
                $"La tienda respondió una redirección sin destino utilizable y la lectura de {store.Host} se detuvo.",
            RedirectRefusal.NotHttps =>
                $"La tienda redirigió la lectura a una dirección sin https ({target}) y sólo se siguen redirecciones https.",
            RedirectRefusal.OutsideStore =>
                $"La tienda redirigió la lectura fuera de {store.Host} ({target}) y sólo se siguen redirecciones del mismo host.",
            RedirectRefusal.TooManyHops =>
                $"La tienda encadenó más de {RedirectTarget.MaximumHops} redirecciones para la misma lectura y la ingesta se detuvo para no entrar en un bucle.",
            RedirectRefusal.NotASheet =>
                $"La tienda redirigió la ficha de {store.StoreName} a una ruta que no es una ficha de producto ({target}) y la ingesta no la sigue.",
            _ => $"La tienda redirigió la lectura de {store.Host} y la ingesta no la pudo seguir ({target})."
        });
    }

    // Sólo se siguen los códigos que conservan la lectura; cualquier otro 3xx se explica en vez de convertirse en
    // una lectura distinta que nadie validó contra robots.txt ni contra la regla de la ficha.
    public static StoreRedirectException UnsupportedStatus(int status, StoreContext store)
    {
        ArgumentNullException.ThrowIfNull(store);
        return new StoreRedirectException(
            $"La tienda respondió {status}, que no es una redirección que la ingesta siga (301, 302, 307 o 308): la lectura de {store.Host} se detuvo.");
    }
}
