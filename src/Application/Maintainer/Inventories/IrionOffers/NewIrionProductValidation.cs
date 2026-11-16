using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Maintainer.Inventories.IrionOffers;

// Se vuelve a leer al confirmar, incluso después de la E/S externa. Este helper nunca deja entidades preparadas ni las guarda.
public static class NewIrionProductValidation
{
    private static readonly Regex Source = new(@"\Ahttps://irion\.cl/products/[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*\z",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Root = new(@"\Ahttps://irion\.cl(?::443)?/?\z",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

    public static async Task<(string? Error, IrionOfferInspectionDto? Inspection)> InspectAsync(
        IApplicationDbContext context, NewIrionProductDetails? p, Guid storeId, string sourceUrl, string variantId, CancellationToken token, bool trackProduct = false)
    {
        token.ThrowIfCancellationRequested();
        if (p is null || p.NewProductId == Guid.Empty || string.IsNullOrWhiteSpace(p.ProductName) || p.ProductName.Length > 200 ||
            p.ManufacturerId == Guid.Empty || p.FranchiseId == Guid.Empty || p.SerieId == Guid.Empty || p.ProductHasBase is null ||
            string.IsNullOrWhiteSpace(p.ProductTargetAge) || p.ProductTargetAge.Length > 100 ||
            string.IsNullOrWhiteSpace(p.ProductSize) || p.ProductSize.Length > 100 || string.IsNullOrWhiteSpace(p.ProductDescription) ||
            p.ProductReleaseDate is null || p.ProductReleaseDate == default(DateOnly) || p.CategoryIds is null ||
            p.CategoryIds.Count == 0 || p.CategoryIds.Contains(Guid.Empty) || p.CategoryIds.Distinct().Count() != p.CategoryIds.Count)
            return ("Los datos del producto nuevo no son válidos.", null);
        if (storeId == Guid.Empty || sourceUrl is null || sourceUrl.Length > 2048 || !Source.IsMatch(sourceUrl) ||
            string.IsNullOrEmpty(variantId) || variantId.Length > 19 || variantId.Any(c => c is < '0' or > '9') ||
            !long.TryParse(variantId, NumberStyles.None, CultureInfo.InvariantCulture, out var variant) || variant <= 0)
            return ("Los datos de la oferta de Irion no son válidos.", null);
        if (!await context.Manufacturers.AsNoTracking().AnyAsync(m => m.ManufacturerId == p.ManufacturerId && m.IsActive, token) ||
            !await context.Franchises.AsNoTracking().AnyAsync(f => f.FranchiseId == p.FranchiseId && f.IsActive, token) ||
            (p.SerieId is not null && !await context.Series.AsNoTracking().AnyAsync(s => s.SerieId == p.SerieId && s.IsActive && s.FranchiseId == p.FranchiseId, token)) ||
            await context.Categories.AsNoTracking().CountAsync(c => p.CategoryIds.Contains(c.CategoryId) && c.IsActive, token) != p.CategoryIds.Count)
            return ("Selecciona referencias de catálogo activas y una serie de la franquicia seleccionada.", null);
        var store = await context.Stores.AsNoTracking().SingleOrDefaultAsync(s => s.StoreId == storeId, token);
        if (store is null || !store.IsActive || store.StoreWebSite is not { Length: <= 2048 } website ||
            !Uri.TryCreate(website, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || uri.Host != "irion.cl" ||
            !uri.IsDefaultPort || uri.UserInfo.Length != 0 || !Root.IsMatch(website))
            return ("Selecciona una tienda activa de Irion con una URL válida.", null);

        var products = trackProduct ? context.Products : context.Products.AsNoTracking();
        var product = await products.SingleOrDefaultAsync(x => x.ProductId == p.NewProductId, token);
        // Usa la regla de igualdad de nombres existente; la colación de SQL Server sigue siendo la autoridad al guardar.
        var nameOccupied = await context.Products.AsNoTracking().AnyAsync(x => x.ProductName == p.ProductName && x.ProductId != p.NewProductId, token);
        var inventory = await context.Inventories.AsNoTracking().SingleOrDefaultAsync(x => x.ProductId == p.NewProductId && x.StoreId == storeId, token);
        var matching = false;
        if (product is not null && product.IsActive && product.ProductName == p.ProductName && product.ManufacturerId == p.ManufacturerId &&
            product.FranchiseId == p.FranchiseId && product.SerieId == p.SerieId && product.ProductHasBase == p.ProductHasBase &&
            product.ProductTargetAge == p.ProductTargetAge && product.ProductSize == p.ProductSize &&
            product.ProductDescription == p.ProductDescription && product.ProductReleaseDate == p.ProductReleaseDate)
        {
            var categories = await context.ProductCategories.AsNoTracking().Where(c => c.ProductId == p.NewProductId)
                .Select(c => c.CategoryId).ToListAsync(token);
            matching = categories.Count == p.CategoryIds.Count && categories.ToHashSet().SetEquals(p.CategoryIds);
        }
        var outcome = nameOccupied ? "Conflict" : product is null && inventory is null ? "Create" :
            matching && inventory is not null && inventory.SourceUrl == sourceUrl && inventory.ExternalVariantId == variantId ? "Update" : "Conflict";
        var message = outcome == "Conflict" ? "El identificador, nombre o vinculación del producto nuevo ya está ocupado o no coincide." :
            inventory is { IsActive: false } ? "El inventario está deshabilitado y permanecerá deshabilitado." : null;
        return (null, new IrionOfferInspectionDto(outcome, p.NewProductId, storeId, sourceUrl, variantId,
            inventory?.InventoryId, inventory is null ? null : Convert.ToBase64String(inventory.RowVersion), inventory?.IsActive, message));
    }
}
