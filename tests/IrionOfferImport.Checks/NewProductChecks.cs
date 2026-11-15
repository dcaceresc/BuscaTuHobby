using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.IrionOffers;
using Application.Maintainer.Inventories.Queries.GetIrionNewProductOptions;
using Application.Maintainer.Inventories.Queries.InspectNewIrionProductOffer;
using Domain.Entities;
using Infrastructure.Services;

internal static class NewProductChecks
{
    public static async Task Run(Action<bool, string> check)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, NewProductContext>();
        var fake = (NewProductContext)(object)context;
        var maker = Manufacturer.Create("Maker");
        var franchise = Franchise.Create("Franchise");
        var category = Category.Create("Category", "", 1, "category");
        var second = Category.Create("Second", "", 2, "second");
        var serie = Serie.Create("Serie", franchise.FranchiseId);
        var store = Store.Create("Irion", "https://irion.cl/", "", 0, "irion");
        fake.Manufacturers.Add(maker); fake.Franchises.Add(franchise);
        fake.Categories.AddRange([category, second]); fake.Series.Add(serie); fake.Stores.Add(store);
        var details = new NewIrionProductDetails(Guid.NewGuid(), "Model", maker.ManufacturerId, franchise.FranchiseId,
            serie.SerieId, false, "15+", "20 cm", "Description", new DateOnly(2026, 1, 1), [category.CategoryId, second.CategoryId]);
        var request = new InspectNewIrionProductOffer(details, store.StoreId, "https://irion.cl/products/model", "123");
        var handler = new InspectNewIrionProductOfferHandler(context, new ApiResponseService());
        async Task Inspect(InspectNewIrionProductOffer input, string? outcome, string? error = null)
        {
            fake.CaptureNewPersisted();
            var before = fake.State();
            var persisted = fake.NewPersisted;
            var result = await handler.Handle(input, default);
            check(result.Success == (outcome is not null) && result.Message == error, "New inspection exact envelope");
            if (outcome is null) check(result.Data is null, "Rejected new inspection has no data");
            else
            {
                var row = fake.Rows.SingleOrDefault(r => r.ProductId == input.Product.NewProductId && r.StoreId == input.StoreId);
                check(result.Data is { } d && d.Outcome == outcome && d.ProductId == input.Product.NewProductId &&
                    d.StoreId == input.StoreId && d.SourceUrl == input.SourceUrl && d.ExternalVariantId == input.ExternalVariantId &&
                    d.InventoryId == row?.InventoryId && d.RowVersion == (row is null ? null : Convert.ToBase64String(row.RowVersion)) &&
                    d.IsActive == row?.IsActive, "New inspection exact typed identity/version");
            }
            check(fake.State() == before && fake.NewPersisted == persisted && fake.Saves == 0,
                "New inspection preserves tracked and stored products/categories/inventory; Save0");
        }
        await Inspect(request, "Create");
        await Inspect(request with { Product = details with { SerieId = null, ProductHasBase = true } }, "Create");
        await Inspect(request with { ExternalVariantId = "9223372036854775807" }, "Create");
        const string invalid = "Los datos del producto nuevo no son válidos.";
        foreach (var bad in new[] {
            details with { NewProductId = Guid.Empty }, details with { ProductName = null! }, details with { ProductName = " " },
            details with { ProductName = new string('x', 201) }, details with { ProductTargetAge = "" },
            details with { ProductTargetAge = new string('x', 101) }, details with { ProductSize = " " },
            details with { ProductSize = new string('x', 101) }, details with { ProductDescription = " " },
            details with { ProductReleaseDate = null }, details with { ProductReleaseDate = default(DateOnly) },
            details with { ProductHasBase = null }, details with { ManufacturerId = Guid.Empty }, details with { FranchiseId = Guid.Empty },
            details with { SerieId = Guid.Empty }, details with { CategoryIds = null! }, details with { CategoryIds = [] },
            details with { CategoryIds = [Guid.Empty] }, details with { CategoryIds = [category.CategoryId, category.CategoryId] } })
            await Inspect(request with { Product = bad }, null, invalid);
        await Inspect(request with { Product = null! }, null, invalid);
        await Inspect(request with { Product = details with { ProductName = new string('x', 200), ProductTargetAge = new string('x', 100), ProductSize = new string('x', 100) } }, "Create");
        const string refs = "Selecciona referencias de catálogo activas y una serie de la franquicia seleccionada.";
        foreach (var bad in new[] { details with { ManufacturerId = Guid.NewGuid() }, details with { FranchiseId = Guid.NewGuid() },
            details with { SerieId = Guid.NewGuid() }, details with { CategoryIds = [Guid.NewGuid()] } })
            await Inspect(request with { Product = bad }, null, refs);
        maker.ToggleActive(); await Inspect(request, null, refs); maker.ToggleActive();
        franchise.ToggleActive(); await Inspect(request, null, refs); franchise.ToggleActive();
        category.ToggleActive(); await Inspect(request, null, refs); category.ToggleActive();
        serie.ToggleActive(); await Inspect(request, null, refs); serie.ToggleActive();
        serie.Update("Serie", Guid.NewGuid()); await Inspect(request, null, refs); serie.Update("Serie", franchise.FranchiseId);
        foreach (var source in new[] { "", "https://other.cl/products/model", request.SourceUrl + "?x=1", request.SourceUrl + "/" })
            await Inspect(request with { SourceUrl = source }, null, "Los datos de la oferta de Irion no son válidos.");
        foreach (var variant in new[] { "0", "-1", "+1", " 1", "1.0", "9223372036854775808" })
            await Inspect(request with { ExternalVariantId = variant }, null, "Los datos de la oferta de Irion no son válidos.");
        await Inspect(request with { StoreId = Guid.Empty }, null, "Los datos de la oferta de Irion no son válidos.");
        await Inspect(request with { StoreId = Guid.NewGuid() }, null, "Selecciona una tienda activa de Irion con una URL válida.");
        store.ToggleActive(); await Inspect(request, null, "Selecciona una tienda activa de Irion con una URL válida."); store.ToggleActive();
        store.Update("Irion", "https://irion.cl/products/model", "", 0, "irion");
        await Inspect(request, null, "Selecciona una tienda activa de Irion con una URL válida.");
        store.Update("Irion", "https://irion.cl/", "", 0, "irion");

        var product = Product.CreateWithProvidedId(details.NewProductId, details.ProductName, details.ManufacturerId, details.FranchiseId,
            details.SerieId, false, details.ProductTargetAge, details.ProductSize, details.ProductDescription, details.ProductReleaseDate!.Value);
        check(product.ProductId == details.NewProductId && product.IsActive && product.ProductViewCount == 0, "Provided ID factory identity/defaults");
        try { Product.CreateWithProvidedId(Guid.Empty, "", Guid.Empty, Guid.Empty, null, false, "", "", "", default); check(false, "Empty factory ID rejected"); }
        catch (ArgumentException) { check(true, "Empty factory ID rejected"); }
        var old = Product.Create(" raw ", Guid.Empty, Guid.Empty, null, false, "", "", "", default);
        check(old.ProductId != Guid.Empty && old.ProductId != product.ProductId && old.ProductName == " raw " &&
            old.ManufacturerId == Guid.Empty && old.FranchiseId == Guid.Empty && old.SerieId is null && !old.ProductHasBase &&
            old.ProductTargetAge == "" && old.ProductSize == "" && old.ProductDescription == "" && old.ProductReleaseDate == default &&
            old.IsActive && old.ProductViewCount == 0, "Old factory GUID and permissive field semantics preserved");
        fake.Products.Add(product);
        fake.ProductCategories.AddRange(details.CategoryIds.Select(product.AssignCategory));
        await Inspect(request, "Conflict"); // Occupied ID alone is never adopted.
        var row = Inventory.CreateImportedOffer(product.ProductId, store.StoreId, 100, 0, 0, request.SourceUrl, "123", true, DateTime.UtcNow);
        typeof(Inventory).GetProperty(nameof(Inventory.RowVersion))!.SetValue(row, new byte[] { 1, 2, 3 });
        fake.Rows.Add(row);
        await Inspect(request, "Update");
        typeof(Product).GetProperty(nameof(Product.ProductImages))!.SetValue(product, new List<ProductImage> { product.AssignImage(1) });
        await Inspect(request, "Update"); // Images added later are outside the retry input scope.
        var otherMaker = Manufacturer.Create("Other"); fake.Manufacturers.Add(otherMaker);
        var otherFranchise = Franchise.Create("Other"); fake.Franchises.Add(otherFranchise);
        var otherSerie = Serie.Create("Other", franchise.FranchiseId); fake.Series.Add(otherSerie);
        foreach (var changed in new[] { details with { ManufacturerId = otherMaker.ManufacturerId },
            details with { FranchiseId = otherFranchise.FranchiseId, SerieId = null }, details with { SerieId = otherSerie.SerieId } })
            await Inspect(request with { Product = changed }, "Conflict");
        fake.Manufacturers.Remove(otherMaker); fake.Franchises.Remove(otherFranchise); fake.Series.Remove(otherSerie);
        await Inspect(request with { SourceUrl = request.SourceUrl + "-other" }, "Conflict");
        var otherStore = Store.Create("Other", "https://irion.cl", "", 0, "other"); fake.Stores.Add(otherStore);
        await Inspect(request with { StoreId = otherStore.StoreId }, "Conflict"); fake.Stores.Remove(otherStore);
        await Inspect(request with { Product = details with { CategoryIds = details.CategoryIds.Reverse().ToArray() } }, "Update");
        row.ToggleActive(); await Inspect(request, "Update");
        foreach (var changed in new[] { details with { ProductName = "Other" }, details with { ManufacturerId = Guid.NewGuid() },
            details with { ProductTargetAge = "16+" }, details with { ProductSize = "21 cm" }, details with { ProductDescription = "Other" },
            details with { ProductReleaseDate = details.ProductReleaseDate.Value.AddDays(1) }, details with { ProductHasBase = true },
            details with { SerieId = null }, details with { CategoryIds = [category.CategoryId] } })
            await Inspect(request with { Product = changed }, changed.ManufacturerId == details.ManufacturerId ? "Conflict" : null,
                changed.ManufacturerId == details.ManufacturerId ? null : refs);
        await Inspect(request with { ExternalVariantId = "124" }, "Conflict");
        product.ToggleActive(); await Inspect(request, "Conflict"); product.ToggleActive();
        await Inspect(request with { Product = details with { NewProductId = Guid.NewGuid() } }, "Conflict");

        var optionsHandler = new GetIrionNewProductOptionsHandler(context, new ApiResponseService());
        var inactive = Manufacturer.Create("Inactive"); inactive.ToggleActive(); fake.Manufacturers.Add(inactive);
        var inactiveFranchise = Franchise.Create("Inactive"); inactiveFranchise.ToggleActive(); fake.Franchises.Add(inactiveFranchise);
        fake.Series.Add(Serie.Create("Hidden", inactiveFranchise.FranchiseId));
        var inactiveSerie = Serie.Create("Inactive", franchise.FranchiseId); inactiveSerie.ToggleActive(); fake.Series.Add(inactiveSerie);
        second.ToggleActive();
        var beforeOptions = fake.State();
        var options = await optionsHandler.Handle(new GetIrionNewProductOptions(), default);
        check(options.Success && options.Message is null && options.Data is { } o &&
            o.Manufacturers.Single() == new NewIrionCatalogOption(maker.ManufacturerId, "Maker") &&
            o.Franchises.Single() == new NewIrionCatalogOption(franchise.FranchiseId, "Franchise") &&
            o.Categories.Single() == new NewIrionCatalogOption(category.CategoryId, "Category") &&
            o.Series.Single() == new NewIrionSerieOption(serie.SerieId, "Serie", franchise.FranchiseId), "Options exact active labels and franchise IDs");
        check(fake.State() == beforeOptions && fake.Saves == 0, "Options read only");
        fake.CaptureNewPersisted();
        var stored = fake.NewPersisted;
        fake.ProductCategories.Add(product.AssignCategory(Guid.NewGuid()));
        check(fake.State() != stored && fake.NewPersisted == stored, "Fixture distinguishes tracked category changes from stored snapshot");
        fake.ProductCategories.RemoveAt(fake.ProductCategories.Count - 1);
        product.SetViewCount(1);
        check(fake.State() != stored && fake.NewPersisted == stored, "Fixture distinguishes tracked product changes from stored snapshot");
        product.SetViewCount(0);
        second.ToggleActive();
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try { await handler.Handle(request, canceled.Token); check(false, "New cancellation"); }
        catch (OperationCanceledException) { check(true, "New cancellation propagates"); }
        try { await optionsHandler.Handle(new GetIrionNewProductOptions(), canceled.Token); check(false, "Options cancellation"); }
        catch (OperationCanceledException) { check(true, "Options cancellation propagates"); }
        using var during = new CancellationTokenSource();
        fake.QueryHook = token => { check(token == during.Token, "New query token forwarded"); during.Cancel(); };
        try { await handler.Handle(request, during.Token); check(false, "Mid-query cancellation"); }
        catch (OperationCanceledException) { check(true, "Mid-query cancellation propagates"); }
        fake.QueryHook = null;
        fake.QueryError = new InvalidOperationException("private");
        await Inspect(request, null, "No se pudo inspeccionar el producto nuevo de Irion.");
        var error = await optionsHandler.Handle(new GetIrionNewProductOptions(), default);
        check(!error.Success && error.Data is null && error.Message == "No se pudieron obtener las opciones del producto nuevo.", "Options safe error");
        check(new[] { typeof(InspectNewIrionProductOfferHandler), typeof(GetIrionNewProductOptionsHandler) }.All(t =>
            t.GetConstructors().Single().GetParameters().All(p => p.ParameterType == typeof(IApplicationDbContext) || p.ParameterType == typeof(IApiResponseService))),
            "Both new handlers have no HTTP/extractor dependency");
    }
}

public class NewProductContext : InspectionContext
{
    public List<Manufacturer> Manufacturers { get; } = [];
    public List<Franchise> Franchises { get; } = [];
    public List<Category> Categories { get; } = [];
    public List<Serie> Series { get; } = [];
    public List<ProductCategory> ProductCategories { get; } = [];
    public string NewPersisted { get; private set; } = "";
    public string State() => JsonSerializer.Serialize(new { Products, ProductCategories, Rows, Stores, Manufacturers, Franchises, Categories, Series });
    public void CaptureNewPersisted() => NewPersisted = State();
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        void Query(CancellationToken token) { QueryHook?.Invoke(token); token.ThrowIfCancellationRequested(); if (QueryError is not null) throw QueryError; }
        return method!.Name switch
        {
            "get_Manufacturers" => new InspectionSet<Manufacturer>(Manufacturers, Query),
            "get_Franchises" => new InspectionSet<Franchise>(Franchises, Query),
            "get_Categories" => new InspectionSet<Category>(Categories, Query),
            "get_Series" => new InspectionSet<Serie>(Series, Query),
            "get_ProductCategories" => new InspectionSet<ProductCategory>(ProductCategories, Query),
            _ => base.Invoke(method, args)
        };
    }
}
