using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Seguimiento acotado de las redirecciones de una lectura de ingesta. El transporte desactiva las redirecciones
/// automáticas porque seguirlas a ciegas saltaría <c>robots.txt</c> y el limitador del dominio; aquí se siguen a mano,
/// con un tope de saltos, y cada salto se vuelve a validar antes de pedirse.
/// </summary>
internal static class StoreRedirectFollower
{
    // Códigos que conservan la lectura y por eso se pueden seguir. 303 cambia la petición, así que se rechaza en vez
    // de convertirse en otra lectura que nadie validó.
    private static bool IsFollowable(int status) => status is 301 or 302 or 307 or 308;

    /// <summary>
    /// Envía la lectura y sigue los saltos permitidos. <paramref name="send"/> hace una sola petición: la envuelve el
    /// llamador con el limitador y, en el descubrimiento, con el presupuesto, así que cada salto cuenta como una
    /// petición más y nunca hay dos en vuelo contra el mismo host.
    /// </summary>
    public static async Task<HttpResponseMessage> SendAsync(
        StoreContext store,
        string url,
        RobotsTxtPolicy policy,
        Func<string, CancellationToken, Task<HttpResponseMessage>> send,
        Func<string, bool>? isSheetTarget,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(send);

        var current = url;
        for (var followed = 0; ; followed++)
        {
            var response = await send(current, cancellationToken);
            var status = (int)response.StatusCode;
            if (status is < 300 or >= 400) return response;

            // Un 3xx que no se puede seguir se explica en español aquí: dejarlo llegar al llamador lo convertiría en
            // un "no fue exitoso" sin el motivo que la tienda está dando.
            if (!IsFollowable(status))
            {
                response.Dispose();
                throw StoreRedirectException.UnsupportedStatus(status, store);
            }

            var location = response.Headers.Location?.OriginalString;
            response.Dispose();
            var decision = RedirectTarget.Resolve(store, current, location, followed, isSheetTarget);
            if (!decision.Allowed) throw StoreRedirectException.For(decision.Refusal, store, location);

            var target = decision.Target!;
            var targetPath = new Uri(target).AbsolutePath;

            // Cada salto vuelve a pasar por robots.txt con su ruta nueva: la primera ruta autorizada no autoriza el
            // destino, y una ruta excluida no se pide aunque el salto anterior fuera válido.
            if (!policy.Allows(targetPath))
                throw new StorePathDisallowedException(store.StoreName, store.RequestHost, targetPath);

            current = target;
        }
    }
}
