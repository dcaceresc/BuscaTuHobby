using System.Reflection;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Commands.ConfirmNewIrionProductOffer;
using Application.Maintainer.Inventories.IrionOffers;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

internal static class NewProductConfirmationChecks
{
    private const string Source = "https://irion.cl/products/model";
    private const string Generic = "No se pudo confirmar la oferta de Irion.";
    public static async Task Run(Action<bool, string> check)
    {
        async Task Reject(Action<Fixture> arrange, string? outcome = null, string? message = Generic, int calls = 1, int saves = 0)
        {
            var f = new Fixture();
            arrange(f);
            f.Fake.CaptureNewPersisted();
            var stored = f.Fake.NewPersisted;
            var tracked = f.Fake.State();
            var result = await f.Handler.Handle(f.Request, default);
            check(!result.Success && result.Message == message && result.Data?.Outcome == outcome, $"New confirm rejection: expected {outcome}/{message}; got {result.Data?.Outcome}/{result.Message}");
            check(f.Extractor.Calls == calls && f.Fake.Saves == saves && f.Fake.NewPersisted == stored,
                "New confirm rejected graph persisted unchanged and exact call counts");
            if (saves == 0 && f.Extractor.After is null) check(f.Fake.State() == tracked, "New confirm rejection stages nothing");
            if (outcome == "RefreshRequired") check(result.Data?.FreshPreview == f.Extractor.Preview && result.Data.Inspection is not null,
                "New confirm refresh contains current preview and inspection");
            if (saves != 0) check(f.Fake.State() != stored, "Failed save tracked graph is not persisted graph");
        }
        var f = new Fixture();
        var start = DateTime.UtcNow;
        f.Fake.SaveHook = token => check(f.Fake.Products.Count == 1 && f.Fake.ProductCategories.Count == 2 && f.Fake.Rows.Count == 1,
            "All three groups staged at first and only save");
        var saved = await f.Handler.Handle(f.Request, default);
        var row = f.Fake.Rows.Single();
        var p = f.Fake.Products.Single();
        check(saved.Success && saved.Message is null && saved.Data?.Outcome == "Saved" && saved.Data.InventoryId == row.InventoryId &&
            saved.Data.Inspection?.ProductId == f.Request.Product.NewProductId, "New confirm typed IDs");
        check(p.ProductId == f.Request.Product.NewProductId && p.ProductName == "Manual name" && !p.ProductHasBase &&
            p.ManufacturerId == f.Request.Product.ManufacturerId && p.FranchiseId == f.Request.Product.FranchiseId && p.SerieId is null &&
            p.ProductTargetAge == "15+" && p.ProductSize == "20cm" && p.ProductDescription == "Human description" &&
            p.ProductReleaseDate == new DateOnly(2020, 1, 2) && p.IsActive && p.ProductViewCount == 0 &&
            (p.ProductImages is null || p.ProductImages.Count == 0) && f.Fake.ProductCategories.All(c => c.ProductId == p.ProductId) &&
            f.Fake.ProductCategories.Select(c => c.CategoryId).ToHashSet().SetEquals(f.Request.Product.CategoryIds), "Exact provided manual fields, categories, zero images");
        check(row.ProductId == p.ProductId && row.StoreId == f.Request.StoreId && row.Price == 199 && row.OriginalPrice == 200 &&
            row.DiscountPercentage == 1 && row.Availability == false && row.IsActive && row.SourceUrl == Source && row.ExternalVariantId == "123" &&
            row.ObservedAtUtc >= start && row.ObservedAtUtc <= DateTime.UtcNow && row.ObservedAtUtc.Value.Kind == DateTimeKind.Utc &&
            f.Fake.Saves == 1 && f.Extractor.Calls == 1 && f.Fake.NewPersisted == f.Fake.State(), "Selected-not-first, server rounding UTC and single graph save");
        var before = f.Fake.NewPersisted;
        var retry = await f.Handler.Handle(f.Request, default);
        check(retry.Success && retry.Data?.Outcome == "AlreadySaved" && retry.Data.InventoryId == row.InventoryId &&
            f.Fake.Saves == 1 && f.Extractor.Calls == 2 && f.Fake.State() == before, "Stale create retry has no extra entities or save");
        SetVersion(row);
        row.ToggleActive();
        f.Request = f.Request with { ExpectedInventoryId = row.InventoryId, ExpectedRowVersion = Convert.ToBase64String(row.RowVersion) };
        f.Offer(100, null);
        var metadata = JsonSerializer.Serialize(new { f.Fake.Products, f.Fake.ProductCategories });
        var update = await f.Handler.Handle(f.Request, default);
        check(update.Success && update.Data?.Outcome == "Saved" && !row.IsActive && row.Price == 100 && row.OriginalPrice == 0 &&
            f.Fake.Saves == 2 && f.Fake.Products.Count == 1 && f.Fake.ProductCategories.Count == 2 && f.Fake.Rows.Count == 1 &&
            JsonSerializer.Serialize(new { f.Fake.Products, f.Fake.ProductCategories }) == metadata, "Update preserves product/categories and disabled inventory");
        foreach (var original in new decimal?[] { null, 0, int.MaxValue })
        {
            var boundary = new Fixture(); boundary.Offer(original == int.MaxValue ? int.MaxValue : 100, original);
            var result = await boundary.Handler.Handle(boundary.Request, default);
            check(result.Success && boundary.Fake.Rows.Single().OriginalPrice == (original ?? 0) && boundary.Fake.Saves == 1,
                "New confirm reference conventions and Int32 boundary");
        }
        foreach (var bad in new[] { f.Request.Product with { ProductName = "" }, f.Request.Product with { ProductHasBase = null },
            f.Request.Product with { ProductReleaseDate = null }, f.Request.Product with { CategoryIds = [] },
            f.Request.Product with { ProductSize = new string('x', 101) } })
            await Reject(x => x.Request = x.Request with { Product = bad }, message: "Los datos del producto nuevo no son válidos.", calls: 0);
        await Reject(x => x.Fake.Categories[0].ToggleActive(), message: "Selecciona referencias de catálogo activas y una serie de la franquicia seleccionada.", calls: 0);
        await Reject(x => x.Request = x.Request with { Product = x.Request.Product with { SerieId = Guid.NewGuid() } },
            message: "Selecciona referencias de catálogo activas y una serie de la franquicia seleccionada.", calls: 0);
        await Reject(x => x.Fake.Stores[0].ToggleActive(), message: "Selecciona una tienda activa de Irion con una URL válida.", calls: 0);
        await Reject(x => x.Request = x.Request with { ExpectedRowVersion = "bad" }, "Conflict", null, 0);
        await Reject(x => x.Request = x.Request with { ExpectedInventoryId = Guid.NewGuid(), ExpectedRowVersion = "AQID" }, "Conflict", null, 0);
        await Reject(x => x.Request = x.Request with { ExpectedInventoryId = Guid.NewGuid(), ExpectedRowVersion = "AAAAAAAAAAA= " }, "Conflict", null, 0);
        await Reject(x => x.Seed(false), "Conflict", null, 0);
        foreach (var change in new Func<NewIrionProductDetails, NewIrionProductDetails>[] {
            d => d with { ProductDescription = "Changed" }, d => d with { ProductName = "Other" },
            d => d with { ProductTargetAge = "16+" }, d => d with { ProductSize = "21cm" },
            d => d with { ProductHasBase = true }, d => d with { ProductReleaseDate = new DateOnly(2021, 1, 1) } })
            await Reject(x => { x.Seed(); x.Request = x.Request with { Product = change(x.Request.Product) }; }, "Conflict", null, 0);
        await Reject(x => { x.Seed(); x.Request = x.Request with { Product = x.Request.Product with { CategoryIds = [x.Request.Product.CategoryIds[0]] } }; }, "Conflict", null, 0);
        await Reject(x => { x.Seed(); x.Request = x.Request with { Product = x.Request.Product with { NewProductId = Guid.NewGuid() } }; }, "Conflict", null, 0);
        await Reject(x => { x.Seed(); x.Offer(100, null); }, "Conflict", null);
        await Reject(x => { x.Seed(); x.Request = x.Request with { ExpectedInventoryId = x.Fake.Rows[0].InventoryId, ExpectedRowVersion = "AAAAAAAAAAA=" }; x.Offer(100, null); }, "Conflict", null);
        foreach (var changed in new Func<IrionProductPreviewDto, IrionProductPreviewDto>[] {
            v => v with { SourceUrl = Source + "-other" }, v => v with { Currency = "USD" },
            v => v with { Variants = [] }, v => v with { Variants = [v.Variants[1] with { Price = 198 }] },
            v => v with { Variants = [v.Variants[1] with { OriginalPrice = 201 }] },
            v => v with { Variants = [v.Variants[1] with { Available = true }] } })
            await Reject(x => x.Extractor.Preview = changed(x.Extractor.Preview), "RefreshRequired", null);
        foreach (var tuple in new[] { (0m, 0m), (-1m, 0m), (1.5m, 0m), ((decimal)int.MaxValue + 1, 0m),
            (100m, -1m), (100m, 99m), (100m, 100.5m), (100m, (decimal)int.MaxValue + 1) })
            await Reject(x => x.Offer(tuple.Item1, tuple.Item2));
        await Reject(x => { x.Extractor.Preview = x.Extractor.Preview with { Currency = "USD" }; x.Request = x.Request with { ExpectedOffer = x.Request.ExpectedOffer with { Currency = "USD" } }; });
        await Reject(x => x.Extractor.Error = new TimeoutException("private"));
        await Reject(x => x.Extractor.Error = new InvalidOperationException("private"));
        await Reject(x => x.Extractor.Error = new StoreRateLimitException("Irion"),
            message: "Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.");
        await Reject(x => x.Extractor.After = () => x.Fake.Categories[0].ToggleActive(),
            message: "Selecciona referencias de catálogo activas y una serie de la franquicia seleccionada.");
        await Reject(x => x.Extractor.After = () => x.Seed(false), "Conflict", null);
        await Reject(x => x.Extractor.After = () => { x.Seed(); x.Fake.ProductCategories.RemoveAt(0); }, "Conflict", null);
        await Reject(x => x.Extractor.After = () => {
            x.Seed(); var d = x.Request.Product;
            x.Fake.Products[0].Update("Concurrent name", d.ManufacturerId, d.FranchiseId, d.SerieId, false,
                d.ProductTargetAge, d.ProductSize, d.ProductDescription, d.ProductReleaseDate!.Value,
                x.Fake.Products[0].Grade, x.Fake.Products[0].Scale, x.Fake.Products[0].PartNumber);
        }, "Conflict", null);
        foreach (var error in new DbUpdateException[] { new("private"), new DbUpdateConcurrencyException("private") })
            await Reject(x => x.Fake.SaveError = error, "Conflict", null, saves: 1);
        foreach (var pre in new[] { true, false })
        {
            var cancel = new Fixture(); using var cts = new CancellationTokenSource();
            if (pre) cts.Cancel(); else cancel.Extractor.After = cts.Cancel;
            try { await cancel.Handler.Handle(cancel.Request, cts.Token); check(false, "New confirm cancellation"); }
            catch (OperationCanceledException) { check(true, "New confirm cancellation propagated"); }
            check(cancel.Fake.Saves == 0 && cancel.Fake.Products.Count == 0 && cancel.Fake.Rows.Count == 0 &&
                cancel.Extractor.Calls == (pre ? 0 : 1), "Cancellation no staging/save/retry");
            if (!pre) check(cancel.Extractor.Token == cts.Token, "Extractor cancellation forwarded");
        }
    }
    private static void SetVersion(Inventory row) => typeof(Inventory).GetProperty(nameof(Inventory.RowVersion))!.SetValue(row, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
    private sealed class Fixture
    {
        public IApplicationDbContext Context { get; } = DispatchProxy.Create<IApplicationDbContext, NewProductContext>();
        public NewProductContext Fake => (NewProductContext)(object)Context;
        public Extractor Extractor { get; } = new();
        public ConfirmNewIrionProductOffer Request { get; set; }
        public ConfirmNewIrionProductOfferHandler Handler { get; }
        public Fixture()
        {
            var maker = Manufacturer.Create("Maker"); var franchise = Franchise.Create("Franchise");
            var a = Category.Create("A", "", 1, "a"); var b = Category.Create("B", "", 2, "b");
            var store = Store.Create("Irion", "https://irion.cl/", "", 0, "irion");
            Fake.Manufacturers.Add(maker); Fake.Franchises.Add(franchise); Fake.Categories.AddRange([a, b]); Fake.Stores.Add(store);
            Request = new(new(Guid.NewGuid(), "Manual name", maker.ManufacturerId, franchise.FranchiseId, null, false,
                "15+", "20cm", "Human description", new DateOnly(2020, 1, 2), [a.CategoryId, b.CategoryId]), store.StoreId, Source, "123", null, null, new("CLP", 199, 200, false));
            Handler = new(Context, new ApiResponseService(), Extractor);
        }
        public void Seed(bool link = true)
        {
            var d = Request.Product;
            var p = Product.CreateWithProvidedId(d.NewProductId, d.ProductName, d.ManufacturerId, d.FranchiseId, d.SerieId,
                d.ProductHasBase!.Value, d.ProductTargetAge, d.ProductSize, d.ProductDescription, d.ProductReleaseDate!.Value);
            Fake.Products.Add(p); Fake.ProductCategories.AddRange(d.CategoryIds.Select(p.AssignCategory));
            if (link)
            {
                var row = Inventory.CreateImportedOffer(p.ProductId, Request.StoreId, 199, 200, 1, Source, "123", false, DateTime.UtcNow);
                SetVersion(row); Fake.Rows.Add(row);
            }
        }
        public void Offer(decimal price, decimal? original)
        {
            Extractor.Preview = Extractor.Preview with { Variants = [Extractor.Preview.Variants[1] with { Price = price, OriginalPrice = original }] };
            Request = Request with { ExpectedOffer = new("CLP", price, original, false) };
        }
    }
    private sealed class Extractor : IStoreProductLoader
    {
        public int Calls { get; private set; }
        public CancellationToken Token { get; private set; }
        public Exception? Error { get; set; }
        public Action? After { get; set; }
        public IrionProductPreviewDto Preview { get; set; } = new(Source, "Remote title", "Vendor", "CLP", "<p>Remote</p>", [], ["https://unused/image"], true,
            [new("999", null, null, true, 1, null, null), new("123", null, null, false, 199, 200, 99)]);
        public Task<IrionProductPreviewDto> LoadAsync(string url, CancellationToken cancellationToken)
        {
            Calls++; Token = cancellationToken; After?.Invoke();
            return Error is null ? Task.FromResult(Preview) : Task.FromException<IrionProductPreviewDto>(Error);
        }
    }
}
