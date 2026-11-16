using System.Globalization;
using System.Text.RegularExpressions;
using Application.Maintainer.Inventories.IrionOffers;

namespace Application.Maintainer.Inventories.Queries.InspectIrionOffer;

public record InspectIrionOffer(Guid ProductId, Guid StoreId, string SourceUrl, string ExternalVariantId)
    : IRequest<ApiResponse<IrionOfferInspectionDto>>;

public class InspectIrionOfferHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<InspectIrionOffer, ApiResponse<IrionOfferInspectionDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private static readonly Regex CanonicalSource = new(
        @"\Ahttps://irion\.cl/products/[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*\z",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex StoreRoot = new(@"\Ahttps://irion\.cl(?::443)?/?\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public async Task<ApiResponse<IrionOfferInspectionDto>> Handle(InspectIrionOffer request, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.ProductId == Guid.Empty || request.StoreId == Guid.Empty ||
                request.SourceUrl is null || request.SourceUrl.Length > 2048 || !CanonicalSource.IsMatch(request.SourceUrl) ||
                string.IsNullOrEmpty(request.ExternalVariantId) || request.ExternalVariantId.Length > 19 ||
                request.ExternalVariantId.Any(c => c is < '0' or > '9') ||
                !long.TryParse(request.ExternalVariantId, NumberStyles.None, CultureInfo.InvariantCulture, out var variant) || variant <= 0)
                return _responseService.Fail<IrionOfferInspectionDto>("Los datos de la oferta de Irion no son válidos.");

            var product = await _context.Products.AsNoTracking()
                .SingleOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);
            if (product is null || !product.IsActive)
                return _responseService.Fail<IrionOfferInspectionDto>("Selecciona un producto activo existente.");

            var store = await _context.Stores.AsNoTracking()
                .SingleOrDefaultAsync(s => s.StoreId == request.StoreId, cancellationToken);
            if (store is null || !store.IsActive || !IsIrionStore(store.StoreWebSite))
                return _responseService.Fail<IrionOfferInspectionDto>("Selecciona una tienda activa de Irion con una URL válida.");

            // Una sola consulta incluye las filas heredadas y desactivadas; la inspección nunca las adopta ni las reactiva.
            var inventory = await _context.Inventories.AsNoTracking().SingleOrDefaultAsync(
                i => i.ProductId == request.ProductId && i.StoreId == request.StoreId, cancellationToken);
            var outcome = inventory is null ? "Create" :
                inventory.SourceUrl == request.SourceUrl && inventory.ExternalVariantId == request.ExternalVariantId ? "Update" : "Conflict";
            var message = outcome == "Conflict" ? "La oferta existente no está vinculada a esta URL y variante de Irion." :
                inventory is { IsActive: false } ? "El inventario está deshabilitado y permanecerá deshabilitado." : null;
            return _responseService.Success(new IrionOfferInspectionDto(outcome, request.ProductId, request.StoreId,
                request.SourceUrl, request.ExternalVariantId, inventory?.InventoryId,
                inventory is null ? null : Convert.ToBase64String(inventory.RowVersion), inventory?.IsActive, message));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<IrionOfferInspectionDto>("No se pudo inspeccionar la oferta de Irion.");
        }
    }

    private static bool IsIrionStore(string website) => website is not null && website.Length <= 2048 &&
        Uri.TryCreate(website, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps &&
        uri.Host == "irion.cl" && uri.IsDefaultPort && uri.UserInfo.Length == 0 && StoreRoot.IsMatch(website);
}
