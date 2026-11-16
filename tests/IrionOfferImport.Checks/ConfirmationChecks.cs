using System.Reflection;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Commands.ConfirmIrionOffer;
using Application.Maintainer.Inventories.Queries.InspectIrionOffer;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

internal static class ConfirmationChecks
{
    private const string Source = "https://irion.cl/products/model-1";
    private const string Generic = "No se pudo confirmar la oferta de Irion.";

    public static async Task Run(Action<bool, string> check)
    {
        async Task Reject(Action<Fixture> arrange, string? outcome = null, string? message = Generic,
            int calls = 1, bool saveAttempt = false)
        {
            var f = new Fixture();
            arrange(f);
            f.Fake.CapturePersisted();
            var persisted = f.Fake.Persisted;
            var tracked = JsonSerializer.Serialize(f.Fake.Rows);
            var result = await f.Handler.Handle(f.Request, default);
            check(!result.Success && result.Message == message && result.Data?.Outcome == outcome,
                "Rejected confirmation exact envelope: " + (outcome ?? message));
            check(f.Extractor.Calls == calls && f.Fake.Saves == (saveAttempt ? 1 : 0) && f.Fake.Persisted == persisted,
                "Rejected confirmation preserves persisted snapshot and exact extraction/save counts");
            if (!saveAttempt) check(JsonSerializer.Serialize(f.Fake.Rows) == tracked,
                "Rejected confirmation does not mutate tracked rows");
            if (outcome == "RefreshRequired")
                check(result.Data?.FreshPreview == f.Extractor.Preview && result.Data.Inspection is not null,
                    "Refresh contains fresh preview and inspection for explicit new confirmation");
        }

        var create = new Fixture();
        var started = DateTime.UtcNow;
        var saved = await create.Handler.Handle(create.Request, default);
        var row = create.Fake.Rows.Single();
        check(saved.Success && saved.Message is null && saved.Data?.Outcome == "Saved" && saved.Data.InventoryId == row.InventoryId,
            "Create returns typed Saved and stored ID");
        check(row.ProductId == create.Product.ProductId && row.StoreId == create.Store.StoreId && row.Price == 199 &&
            row.OriginalPrice == 200 && row.DiscountPercentage == 1 && row.Availability == false && row.IsActive &&
            row.SourceUrl == Source && row.ExternalVariantId == "123" && row.ObservedAtUtc >= started &&
            row.ObservedAtUtc <= DateTime.UtcNow && row.ObservedAtUtc.Value.Kind == DateTimeKind.Utc &&
            create.Fake.Saves == 1 && create.Extractor.Calls == 1 && create.Fake.Persisted == JsonSerializer.Serialize(create.Fake.Rows),
            "Create derives exact selected-not-first values, away-from-zero discount and server UTC metadata");
        var beforeRepeat = create.Fake.Persisted;
        var repeated = await create.Handler.Handle(create.Request, default);
        check(repeated.Success && repeated.Message is null && repeated.Data?.Outcome == "AlreadySaved" &&
            repeated.Data.InventoryId == row.InventoryId && create.Fake.Rows.Count == 1 && create.Fake.Saves == 1 &&
            create.Extractor.Calls == 2 && create.Fake.Persisted == beforeRepeat,
            "Repeated stale-create confirmation returns AlreadySaved without another write or timestamp");

        foreach (var disabled in new[] { false, true })
        {
            var f = new Fixture();
            var existing = f.Link();
            if (disabled) existing.ToggleActive();
            var id = existing.InventoryId;
            var response = await f.Handler.Handle(f.Request, default);
            check(response.Success && response.Data?.Outcome == "Saved" && response.Data.InventoryId == id &&
                f.Fake.Rows.Single().InventoryId == id && existing.IsActive == !disabled && existing.Price == 199 &&
                existing.OriginalPrice == 200 && existing.DiscountPercentage == 1 && existing.Availability == false &&
                f.Fake.Saves == 1 && f.Extractor.Calls == 1, "Update preserves identity and administrative activation");
            var state = f.Fake.Persisted;
            var retry = await f.Handler.Handle(f.Request, default);
            check(retry.Success && retry.Data?.Outcome == "AlreadySaved" && retry.Data.InventoryId == id &&
                f.Fake.Saves == 1 && f.Fake.Persisted == state, "Repeated update avoids second save");
        }
        foreach (var original in new decimal?[] { null, 0, int.MaxValue })
        {
            var f = new Fixture();
            var price = original == int.MaxValue ? int.MaxValue : 100;
            f.SetOffer(price, original);
            var result = await f.Handler.Handle(f.Request, default);
            check(result.Success && result.Data?.Outcome == "Saved" && f.Fake.Rows.Single().Price == price &&
                f.Fake.Rows.Single().OriginalPrice == (original ?? 0) && f.Fake.Rows.Single().DiscountPercentage == 0 &&
                f.Fake.Saves == 1 && f.Extractor.Calls == 1, "Missing/zero original and Int32 boundary accepted");
        }
        await Reject(f => f.Request = f.Request with { ProductId = Guid.Empty }, message: "Los datos de la oferta de Irion no son válidos.", calls: 0);
        await Reject(f => f.Request = f.Request with { SourceUrl = Source + "?x=1" }, message: "Los datos de la oferta de Irion no son válidos.", calls: 0);
        await Reject(f => f.Request = f.Request with { ExternalVariantId = "" }, message: "Los datos de la oferta de Irion no son válidos.", calls: 0);
        foreach (var missing in new[] { false, true })
        {
            await Reject(f => { if (missing) f.Fake.Products.Clear(); else f.Product.ToggleActive(); },
                message: "Selecciona un producto activo existente.", calls: 0);
            await Reject(f => { if (missing) f.Fake.Stores.Clear(); else f.Store.ToggleActive(); },
                message: "Selecciona una tienda activa de Irion con una URL válida.", calls: 0);
        }
        await Reject(f => f.Store.Update("Irion", "https://evil.test/", "", 0, "irion"),
            message: "Selecciona una tienda activa de Irion con una URL válida.", calls: 0);
        await Reject(f => f.Fake.Rows.Add(Inventory.Create(f.Product.ProductId, f.Store.StoreId, 100, 0, 0)), "Conflict", null, 0);
        await Reject(f => f.Link(variant: "124"), "Conflict", null, 0);
        await Reject(f => f.Link(source: Source + "-other"), "Conflict", null, 0);
        await Reject(f => { f.Link(); f.Request = f.Request with { ExpectedInventoryId = Guid.NewGuid() }; }, "Conflict", null);
        await Reject(f => { f.Link(); f.Request = f.Request with { ExpectedRowVersion = Convert.ToBase64String(new byte[8]) }; }, "Conflict", null);
        foreach (var token in new[] { null, "!", "AQ==", Convert.ToBase64String(new byte[9]) })
            await Reject(f => { f.Link(); f.Request = f.Request with { ExpectedRowVersion = token }; }, "Conflict", null, 0);
        await Reject(f => f.Request = f.Request with { ExpectedRowVersion = "AQ==" }, "Conflict", null, 0);
        await Reject(f => f.Request = f.Request with { ExpectedInventoryId = Guid.NewGuid(), ExpectedRowVersion = Convert.ToBase64String(new byte[8]) }, "Conflict", null);
        await Reject(f => { f.Link(); f.Request = f.Request with { ExpectedInventoryId = null, ExpectedRowVersion = null }; }, "Conflict", null);

        await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { SourceUrl = Source + "-other" });
        await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { Variants = [] }, "RefreshRequired", null);
        await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { Variants = [f.Extractor.Preview.Variants[0]] }, "RefreshRequired", null);
        await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { Variants = [f.Extractor.Preview.Variants[1] with { OriginalPrice = null }] }, "RefreshRequired", null);
        await Reject(f => f.Request = f.Request with { ExpectedOffer = null! }, calls: 0);
        await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { Currency = "USD" });
        await Reject(f => f.Request = f.Request with { ExpectedOffer = f.Request.ExpectedOffer with { Currency = "USD" } }, "RefreshRequired", null);
        foreach (var change in new[] { "price", "original", "available" })
            await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { Variants = [f.Extractor.Preview.Variants[1] with
            { Price = change == "price" ? 198 : 199, OriginalPrice = change == "original" ? 201 : 200,
                Available = change == "available" }] }, "RefreshRequired", null);
        await Reject(f => f.Extractor.Preview = f.Extractor.Preview with { Variants = [f.Extractor.Preview.Variants[1], f.Extractor.Preview.Variants[1]] });
        foreach (var price in new decimal[] { 0, -1, 1.5m, (decimal)int.MaxValue + 1 })
            await Reject(f => f.SetOffer(price, null));
        foreach (var original in new decimal[] { -1, 198, 200.5m, (decimal)int.MaxValue + 1 })
            await Reject(f => f.SetOffer(199, original));
        await Reject(f => f.Extractor.Error = new IrionRateLimitException(), message: new IrionRateLimitException().Message);
        await Reject(f => f.Extractor.Error = new IrionProductUrlException(), message: new IrionProductUrlException().Message);
        await Reject(f => f.Extractor.Error = new TimeoutException("private"));
        await Reject(f => f.Extractor.Error = new OperationCanceledException());
        foreach (var failure in new Exception[] { new DbUpdateConcurrencyException("private"), new DbUpdateException("private") })
        {
            await Reject(f => f.Fake.SaveError = failure, "Conflict", null, saveAttempt: true);
            await Reject(f => { f.Link(); f.Fake.SaveError = failure; }, "Conflict", null, saveAttempt: true);
        }
        // Las esperas externas pueden invalidar las selecciones activas o el par actual antes de la mutación.
        await Reject(f => f.Extractor.After = () => f.Product.ToggleActive(), message: "Selecciona un producto activo existente.");
        await Reject(f => f.Extractor.After = () => f.Store.ToggleActive(), message: "Selecciona una tienda activa de Irion con una URL válida.");
        await Reject(f => f.Extractor.After = () => f.Store.Update("Irion", "https://evil.test", "", 0, "irion"),
            message: "Selecciona una tienda activa de Irion con una URL válida.");
        var race = new Fixture();
        race.Extractor.After = () => race.Link(variant: "124");
        var raceResult = await race.Handler.Handle(race.Request, default);
        check(!raceResult.Success && raceResult.Data?.Outcome == "Conflict" && race.Fake.Saves == 0 && race.Extractor.Calls == 1,
            "Identity inserted during external await is not overwritten");
        var versionRace = new Fixture();
        var racingRow = versionRace.Link();
        versionRace.Extractor.After = () =>
            typeof(Inventory).GetProperty(nameof(Inventory.RowVersion))!.SetValue(racingRow, new byte[8]);
        var versionResult = await versionRace.Handler.Handle(versionRace.Request, default);
        check(!versionResult.Success && versionResult.Message is null && versionResult.Data?.Outcome == "Conflict" &&
            versionRace.Fake.Saves == 0 && versionRace.Extractor.Calls == 1 && racingRow.Price == 100,
            "Version changed during external await conflicts before mutation");
        var disabledRace = new Fixture();
        var disabledRow = disabledRace.Link();
        disabledRace.Extractor.After = disabledRow.ToggleActive;
        var disabledResult = await disabledRace.Handler.Handle(disabledRace.Request, default);
        check(disabledResult.Success && disabledResult.Data?.Outcome == "Saved" && !disabledRow.IsActive &&
            disabledRow.Price == 199 && disabledRace.Fake.Saves == 1 && disabledRace.Extractor.Calls == 1,
            "Administrative disable during external await is preserved");
        using (var forwarded = new CancellationTokenSource())
        {
            var f = new Fixture();
            var tokens = new List<CancellationToken>();
            f.Fake.QueryHook = tokens.Add;
            f.Fake.SaveHook = tokens.Add;
            var response = await f.Handler.Handle(f.Request, forwarded.Token);
            check(response.Success && f.Extractor.Token == forwarded.Token && tokens.Count == 8 &&
                tokens.All(t => t == forwarded.Token), "Token forwarded through all seven queries, extraction and save");
        }
        foreach (var beforeHttp in new[] { true, false })
        {
            var f = new Fixture();
            using var cancellation = new CancellationTokenSource();
            if (beforeHttp) cancellation.Cancel(); else f.Extractor.After = cancellation.Cancel;
            try { await f.Handler.Handle(f.Request, cancellation.Token); check(false, "Caller cancellation propagates"); }
            catch (OperationCanceledException) { check(true, "Caller cancellation propagates"); }
            check(f.Fake.Saves == 0 && f.Fake.Rows.Count == 0 && f.Extractor.Calls == (beforeHttp ? 0 : 1), "Canceled confirmation has no write/retry");
            if (!beforeHttp) check(f.Extractor.Token == cancellation.Token, "Cancellation forwarded to extractor");
        }
    }

    private sealed class Fixture
    {
        public IApplicationDbContext Context { get; } = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        public InspectionContext Fake => (InspectionContext)(object)Context;
        public Product Product { get; } = Product.Create("Model", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
        public Store Store { get; } = Store.Create("Irion", "https://irion.cl/", "", 0, "irion");
        public Extractor Extractor { get; } = new();
        public ConfirmIrionOffer Request { get; set; }
        public ConfirmIrionOfferHandler Handler { get; }
        public Fixture()
        {
            Fake.Products.Add(Product);
            Fake.Stores.Add(Store);
            Request = new(Product.ProductId, Store.StoreId, Source, "123", null, null, new("CLP", 199, 200, false));
            Handler = new(Context, new ApiResponseService(), Extractor, new InspectIrionOfferHandler(Context, new ApiResponseService()));
        }
        public Inventory Link(string source = Source, string variant = "123")
        {
            var row = Inventory.CreateImportedOffer(Product.ProductId, Store.StoreId, 100, 0, 0, source, variant, true, DateTime.UtcNow.AddDays(-1));
            typeof(Inventory).GetProperty(nameof(Inventory.RowVersion))!.SetValue(row, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            Fake.Rows.Add(row);
            Request = Request with { ExpectedInventoryId = row.InventoryId, ExpectedRowVersion = Convert.ToBase64String(row.RowVersion) };
            return row;
        }
        public void SetOffer(decimal price, decimal? original)
        {
            Extractor.Preview = Extractor.Preview with { Variants = [Extractor.Preview.Variants[1] with { Price = price, OriginalPrice = original }] };
            Request = Request with { ExpectedOffer = new("CLP", price, original, false) };
        }
    }
    private sealed class Extractor : IIrionProductExtractor
    {
        public int Calls { get; private set; }
        public CancellationToken Token { get; private set; }
        public Action? After { get; set; }
        public Exception? Error { get; set; }
        public IrionProductPreviewDto Preview { get; set; } = new(Source, "Model", "Vendor", "CLP", "", [], [], true,
            [new("999", null, null, true, 1, null, null), new("123", null, null, false, 199, 200, 99)]);
        public Task<IrionProductPreviewDto> ExtractAsync(string url, CancellationToken cancellationToken)
        {
            Calls++;
            Token = cancellationToken;
            if (url != Source) throw new InvalidOperationException("Unexpected extraction URL");
            After?.Invoke();
            return Error is null ? Task.FromResult(Preview) : Task.FromException<IrionProductPreviewDto>(Error);
        }
    }
}
