using System.Reflection;
using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Application.Dashboard.Queries.GetPriceDrops;
using Application.Maintainer.Ingestion;
using Application.Maintainer.Ingestion.Commands.StartIngestionRun;
using Application.Maintainer.Ingestion.Queries.GetIngestionStatus;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

// Automatización de la ingesta: disparo manual, pasada programada, estado y bajadas de precio derivadas del historial
// que ya existe. Todo determinista: la "red" es un doble que cuenta peticiones y la "base" es el contexto en memoria
// de las comprobaciones anteriores. Ni SQL Server ni HTTP real.
internal static class AutomationChecks
{
    // Los contratos puros se comprueban con un instante fijo; los handlers que leen el reloj usan UtcNow relativo a
    // propósito, porque su ventana de frescura y de días se mide contra el ahora real.
    private static readonly DateTime Now = new(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);

    public static async Task Run(Action<bool, string> check)
    {
        SettingsContract(check);
        ScheduleContract(check);
        await TriggerContract(check);
        await ScheduledContract(check);
        SelectionContract(check);
        await PriceDropContract(check);
        await StatusContract(check);
        await WorkerContract(check);
        CompositionContract(check);
    }

    // La automatización nunca se enciende sola: sin configuración queda apagada y cada seis horas.
    private static void SettingsContract(Action<bool, string> check)
    {
        var defaults = IngestionSettings.Parse(null, null);
        check(!defaults.Enabled && defaults.IntervalHours == IngestionSettings.DefaultIntervalHours &&
            defaults.IntervalHours == 6 && defaults.Interval == TimeSpan.FromHours(6),
            "Scheduled ingestion is off and every six hours when nobody configures it");

        foreach (var (enabled, hours, expectedEnabled, expectedHours) in new (string?, string?, bool, int)[]
                 {
                     ("true", "3", true, 3),
                     ("false", "12", false, 12),
                     ("TRUE", " 4 ", true, 4),
                     ("sí", "6", false, 6),
                     ("true", null, true, 6),
                     ("true", "mucho", true, 6),
                     ("true", "0", true, IngestionSettings.MinimumIntervalHours),
                     ("true", "-5", true, IngestionSettings.MinimumIntervalHours),
                     ("true", "999", true, IngestionSettings.MaximumIntervalHours)
                 })
        {
            var parsed = IngestionSettings.Parse(enabled, hours);
            check(parsed.Enabled == expectedEnabled && parsed.IntervalHours == expectedHours,
                "Ingestion:Enabled/Ingestion:IntervalHours exact parsing for " + (enabled ?? "null") + "/" +
                (hours ?? "null"));
        }
    }

    // La planificación de la vuelta: quién se ejecuta y por qué no se ejecuta el resto.
    private static void ScheduleContract(Action<bool, string> check)
    {
        var running = Store("Irion", "irion.cl");
        var idle = Store("LuffyToys", "luffytoys.cl");
        var unconfigured = Store("Sin conector", "sin-conector.cl") with { ConnectorKey = string.Empty };

        var plan = IngestionSchedule.Plan([running, idle, unconfigured], new HashSet<Guid> { running.StoreId });

        check(plan.Count == 3 && plan.Select(entry => entry.Store).SequenceEqual(new[] { running, idle, unconfigured }),
            "The plan describes every store it receives, in the order it received them");
        check(!plan[0].IsDue && plan[0].SkipReason == IngestionSchedule.RunInProgressReason(running) &&
            plan[0].SkipReason == "La tienda Irion ya tiene una ingesta en curso: la vuelta la omite.",
            "A store with an open run is skipped with a Spanish reason: " + plan[0].SkipReason);
        check(plan[1].IsDue && plan[1].SkipReason is null,
            "Only the store with no open run and a connector is due");
        check(!plan[2].IsDue && plan[2].SkipReason == IngestionSchedule.NoConnectorReason(unconfigured) &&
            plan[2].SkipReason ==
            "La tienda Sin conector no tiene conector configurado: no participa de la ingesta programada.",
            "A store without a connector never takes part, even if it reaches the plan");
        check(IngestionSchedule.Plan([], new HashSet<Guid>()).Count == 0,
            "An empty store list schedules nothing");
    }

    // (a) y (b): el disparo manual rechaza la segunda ejecución y la tienda desconocida sin tocar la red.
    private static async Task TriggerContract(Action<bool, string> check)
    {
        var fixture = new AutomationFixture();
        var store = fixture.AddStore("Irion", "irion", "irion.cl", settings: """{"requestBudget":20}""");
        const string url = "https://irion.cl/products/gundam";
        fixture.Discovery.Queue("irion.cl", url);
        fixture.Connector.Sheets[url] = Sheet(url, "RG 1/144 Gundam RX-78-2", "Bandai", "RG-1", 25990);

        // (b) Una tienda que no existe se rechaza antes de cualquier petición.
        await Expect<NotFoundException>(() => fixture.Runner.RunAsync("fantasma", null, default),
            "La tienda fantasma no existe o no tiene ingesta configurada.", "an unknown store");
        check(fixture.Discovery.Calls == 0 && fixture.Connector.Loaded.Count == 0 && fixture.Fake.Saves == 0 &&
            fixture.Fake.ImportRuns.Count == 0 && fixture.Fake.Products.Count == 0,
            "b) An unknown store is refused without discovering, loading or writing anything");

        // (b) Una tienda configurada sin conector tampoco participa: la clave es lo que define la ingesta.
        fixture.AddStore("Sin conector", "sin-conector", "sin-conector.cl", connectorKey: null);
        await Expect<NotFoundException>(() => fixture.Runner.RunAsync("sin-conector", null, default),
            "La tienda sin-conector no existe o no tiene ingesta configurada.", "an unconfigured store");
        check(fixture.Discovery.Calls == 0 && fixture.Connector.Loaded.Count == 0 && fixture.Fake.Saves == 0,
            "b) A store without a connector is refused exactly like an unknown one and never reaches the network");

        // (a) Una ejecución abierta bloquea la segunda para la misma tienda.
        var open = ImportRun.Create(store.StoreId, 10, Now.AddHours(-1));
        fixture.Fake.ImportRuns.Add(open);
        fixture.Fake.Capture();
        var saves = fixture.Fake.Saves;
        await Expect<IngestionConflictException>(() => fixture.Runner.RunAsync("irion", null, default),
            "La tienda Irion ya tiene una ingesta en curso: espera a que termine antes de lanzar otra.",
            "a second run of the same store");
        check(fixture.Discovery.Calls == 0 && fixture.Connector.Loaded.Count == 0 &&
            fixture.Fake.ImportRuns.Count == 1 && fixture.Fake.Saves == saves &&
            open.Status == ImportRunStatus.Running && open.DiscoveredCount == 0 && open.StoredCount == 0,
            "a) An open run blocks the second one without discovering, loading or saving anything");

        // Una ejecución cerrada no bloquea: el bloqueo habla de la ejecución abierta, no del historial.
        open.Fail(Now.AddMinutes(-30));
        var summary = await fixture.Runner.RunAsync("  IRION ", 2, default);
        var written = fixture.Fake.ImportRuns[^1];
        check(summary.ImportRunId == written.ImportRunId && summary.StoreId == store.StoreId &&
            summary.StoreName == "Irion" && summary.Status == ImportRunStatus.Completed &&
            summary.RequestBudget == 2 && written.RequestBudget == 2 &&
            summary.StartedAtUtc.Kind == DateTimeKind.Utc && summary.FinishedAtUtc is not null,
            "The manual trigger answers with the run id and the counters it wrote");
        check(summary.DiscoveredCount == 1 && summary.StoredCount == 1 && summary.SkippedCount == 0 &&
            summary.FailedCount == 0 && fixture.Connector.Loaded.SequenceEqual(new[] { url }) &&
            fixture.Fake.ImportItems.Count(item => item.ImportRunId == summary.ImportRunId) == 1,
            "The manual trigger runs discovery plus processing and returns the real counters");

        // El presupuesto manual nunca sale de los límites: ni cero, ni por encima del configurado por la tienda.
        var zero = new AutomationFixture();
        zero.AddStore("Irion", "irion", "irion.cl", settings: """{"requestBudget":20}""");
        zero.Discovery.Queue("irion.cl");
        var clampedLow = await zero.Runner.RunAsync("irion", 0, default);
        check(clampedLow.RequestBudget == 1 && zero.Fake.ImportRuns.Single().RequestBudget == 1,
            "A manual request budget of zero becomes one request instead of failing the run");

        var high = new AutomationFixture();
        high.AddStore("Irion", "irion", "irion.cl", settings: """{"requestBudget":20}""");
        high.Discovery.Queue("irion.cl");
        var clampedHigh = await high.Runner.RunAsync("irion", 999, default);
        check(clampedHigh.RequestBudget == 20 && high.Fake.ImportRuns.Single().RequestBudget == 20,
            "A manual trigger never exceeds the request budget configured for the store");
    }

    // (c) La vuelta programada: omite la tienda con ejecución abierta, la que no tiene conector y la inactiva.
    private static async Task ScheduledContract(Action<bool, string> check)
    {
        var fixture = new AutomationFixture();
        var running = fixture.AddStore("Irion", "irion", "irion.cl", settings: """{"requestBudget":20}""");
        fixture.AddStore("LuffyToys", "luffytoys", "luffytoys.cl",
            settings: """{"requestBudget":7}""");
        fixture.AddStore("Sin conector", "sin-conector", "sin-conector.cl", connectorKey: null);
        fixture.AddStore("Cerrada", "cerrada", "cerrada.cl", active: false);

        var open = ImportRun.Create(running.StoreId, 5, Now.AddHours(-2));
        fixture.Fake.ImportRuns.Add(open);
        const string url = "https://luffytoys.cl/products/gundam-ex";
        fixture.Discovery.Queue("luffytoys.cl", url);
        fixture.Connector.Sheets[url] = Sheet(url, "[Preventa] GUNDAM EX", "Bandai - Tamashii Nations", null,
            31990);

        var summaries = await fixture.Runner.RunScheduledAsync(default);

        check(summaries.Count == 1 && summaries[0].StoreName == "LuffyToys" &&
            summaries[0].Status == ImportRunStatus.Completed && summaries[0].StoredCount == 1 &&
            summaries[0].RequestBudget == 7 && summaries[0].DiscoveredCount == 1,
            "c) The scheduled pass runs only the eligible store, within its configured budget");
        check(fixture.Discovery.Hosts.SequenceEqual(new[] { "luffytoys.cl" }) &&
            fixture.Connector.Loaded.SequenceEqual(new[] { url }) &&
            fixture.Fake.Products.Count == 1,
            "c) The store with an open run, the store without a connector and the inactive store never touch the network");
        check(open.Status == ImportRunStatus.Running && open.DiscoveredCount == 0 && open.StoredCount == 0 &&
            fixture.Fake.ImportRuns.Count == 2 &&
            fixture.Fake.ImportItems.All(item => item.ImportRunId != open.ImportRunId),
            "c) The open run is neither resumed nor modified: the next round waits for it");
    }

    // (d) La selección de bajadas compara los dos últimos puntos, respeta el umbral y nunca compara una serie corta.
    private static void SelectionContract(Action<bool, string> check)
    {
        var threePoints = Guid.NewGuid();
        var increased = Guid.NewGuid();
        var single = Guid.NewGuid();
        var half = Guid.NewGuid();
        var small = Guid.NewGuid();
        var twoPercent = Guid.NewGuid();
        var full = Guid.NewGuid();
        var zeroBase = Guid.NewGuid();

        var points = new List<PriceDropPoint>
        {
            Point(threePoints, 12000, -3), Point(threePoints, 10000, -2), Point(threePoints, 9000, -1),
            Point(increased, 9000, -2), Point(increased, 10000, -1),
            Point(single, 9500, -1),
            Point(half, 20000, -2), Point(half, 10000, -1),
            Point(small, 10000, -2), Point(small, 9950, -1),
            Point(twoPercent, 10000, -2), Point(twoPercent, 9800, -1),
            Point(full, 5000, -2), Point(full, 0, -1),
            Point(zeroBase, 0, -2), Point(zeroBase, 0, -1)
        };

        var ten = PriceDropSelection.Select(points, 10);
        check(ten.Select(drop => drop.Latest.InventoryId).SequenceEqual(new[] { full, threePoints, half }),
            "d) Only offers whose last two points drop at least 10% are selected, cheapest first: " +
            string.Join(", ", ten.Select(drop => drop.Latest.InventoryId)));
        var latest = ten.Single(drop => drop.Latest.InventoryId == threePoints);
        check(latest.DropPercentage == 10 && latest.PreviousPrice == 10000 &&
            latest.PreviousObservedAtUtc == Now.AddDays(-2) && latest.Latest.Price == 9000 &&
            latest.Latest.ObservedAtUtc == Now.AddDays(-1),
            "d) The comparison uses the latest two points, not the oldest recorded price: " + latest.DropPercentage);
        check(!ten.Any(drop => drop.Latest.InventoryId == increased),
            "d) An increase is not a price drop");
        check(!ten.Any(drop => drop.Latest.InventoryId == single),
            "d) An offer with a single point is ignored instead of comparing a short series");
        check(!ten.Any(drop => drop.Latest.InventoryId == zeroBase),
            "d) A previous price of zero cannot produce a percentage and is ignored");
        check(!ten.Any(drop => drop.Latest.InventoryId == small),
            "d) A 0.5% drop stays out of a 10% threshold");

        check(PriceDropSelection.Select(points, 1).Select(drop => drop.Latest.InventoryId)
                .SequenceEqual(new[] { full, threePoints, twoPercent, half }),
            "d) A 1% threshold includes the 2% drop and keeps the 0.5% one out");
        check(PriceDropSelection.Select(points, 50).Select(drop => drop.Latest.InventoryId)
                .SequenceEqual(new[] { full, half }),
            "d) minPercent filters exactly at the threshold");
        var fifty = PriceDropSelection.Select(points, 50).Single(drop => drop.Latest.InventoryId == half);
        check(fifty.DropPercentage == 50 && fifty.PreviousPrice == 20000,
            "d) The published percentage is the drop between the two latest points");
        check(PriceDropSelection.Select(points, 100).Select(drop => drop.Latest.InventoryId)
                .SequenceEqual(new[] { full }),
            "d) A full drop reaches the 100% threshold");
        check(PriceDropSelection.Select(points, 1000).Select(drop => drop.Latest.InventoryId)
                .SequenceEqual(new[] { full }),
            "d) minPercent above one hundred clamps to one hundred");
        check(PriceDropSelection.Select(points, 0).Select(drop => drop.Latest.InventoryId)
                .SequenceEqual(new[] { full, threePoints, twoPercent, half }),
            "d) minPercent below one clamps to one percent");
        check(PriceDropSelection.Select([], 10).Count == 0 &&
            PriceDropSelection.Select(new[] { Point(single, 1000, -1) }, 1).Count == 0,
            "d) No points or a single point select nothing");
        check(PriceDropSelection.Select(points, int.MinValue).Count == 4 &&
            PriceDropSelection.Select(points, int.MaxValue).Count == 1,
            "d) Extreme thresholds clamp instead of throwing");

        var tieOld = Guid.NewGuid();
        var tieNew = Guid.NewGuid();
        var tie = PriceDropSelection.Select(
        [
            Point(tieOld, 10000, -3), Point(tieOld, 8000, -2),
            Point(tieNew, 10000, -3), Point(tieNew, 8000, -1)
        ], 10);
        check(tie.Select(drop => drop.Latest.InventoryId).SequenceEqual(new[] { tieNew, tieOld }),
            "d) Two offers at the same price are listed newest observation first");
    }

    // (d) La consulta pública: ventana de días, elegibilidad de la ficha de producto y orden barato-más-nuevo.
    private static async Task PriceDropContract(Action<bool, string> check)
    {
        var now = DateTime.UtcNow;
        var fixture = new AutomationFixture();
        var store = fixture.AddStore("Irion", "irion", "irion.cl");
        var closed = fixture.AddStore("Cerrada", "cerrada", "cerrada.cl", active: false);
        var product = fixture.AddProduct("RG 1/144 Gundam RX-78-2");

        var recent = fixture.AddOffer(store, product, 9000, true, now.AddDays(-1));
        fixture.AddHistory(recent, 10000, now.AddDays(-3));
        fixture.AddHistory(recent, 9000, now.AddDays(-1));

        var soldOut = fixture.AddOffer(store, product, 1000, false, now.AddDays(-1));
        fixture.AddHistory(soldOut, 10000, now.AddDays(-3));
        fixture.AddHistory(soldOut, 1000, now.AddDays(-1));

        var olderWindow = fixture.AddOffer(store, product, 8000, true, now.AddDays(-1));
        fixture.AddHistory(olderWindow, 10000, now.AddDays(-40));
        fixture.AddHistory(olderWindow, 8000, now.AddDays(-1));

        var ancient = fixture.AddOffer(store, product, 7000, true, now.AddDays(-1));
        fixture.AddHistory(ancient, 10000, now.AddDays(-4000));
        fixture.AddHistory(ancient, 7000, now.AddDays(-1));

        var tiny = fixture.AddOffer(store, product, 9950, true, now.AddDays(-1));
        fixture.AddHistory(tiny, 10000, now.AddDays(-3));
        fixture.AddHistory(tiny, 9950, now.AddDays(-1));

        var inactive = fixture.AddOffer(store, product, 100, true, now.AddDays(-1), active: false);
        fixture.AddHistory(inactive, 10000, now.AddDays(-3));
        fixture.AddHistory(inactive, 100, now.AddDays(-1));

        var deadStore = fixture.AddOffer(closed, product, 100, true, now.AddDays(-1));
        fixture.AddHistory(deadStore, 10000, now.AddDays(-3));
        fixture.AddHistory(deadStore, 100, now.AddDays(-1));

        var handler = new GetPriceDropsHandler(fixture.Context, new ApiResponseService());

        var windowed = await handler.Handle(new GetPriceDrops(30, 10), default);
        check(windowed.Success && windowed.Message is null &&
            windowed.Data.Select(drop => drop.InventoryId).SequenceEqual(new[] { recent.InventoryId }),
            "d) The public route lists only the eligible drop inside the window: " +
            string.Join(", ", windowed.Data.Select(drop => drop.InventoryId)));
        var drop = windowed.Data.Single();
        check(drop.ProductId == product.ProductId && drop.ProductName == "RG 1/144 Gundam RX-78-2" &&
            drop.StoreName == "Irion" && drop.StoreSlug == "irion" && drop.CurrentPrice == 9000 &&
            drop.PreviousPrice == 10000 && drop.DropPercentage == 10 && drop.ObservedAtUtc == now.AddDays(-1),
            "d) The listed drop publishes the offer, the two prices and the percentage");

        var whole = await handler.Handle(new GetPriceDrops(0, 10), default);
        check(whole.Data.Select(row => row.InventoryId)
                .SequenceEqual(new[] { ancient.InventoryId, olderWindow.InventoryId, recent.InventoryId }),
            "d) days = 0 compares the whole series, cheapest newest first");
        check(!whole.Data.Any(row => row.InventoryId == soldOut.InventoryId ||
                row.InventoryId == inactive.InventoryId || row.InventoryId == deadStore.InventoryId),
            "d) A sold-out offer, a disabled offer and a closed store are not price drops");

        var negative = await handler.Handle(new GetPriceDrops(-5, 10), default);
        check(negative.Data.Select(row => row.InventoryId)
                .SequenceEqual(whole.Data.Select(row => row.InventoryId)),
            "d) A negative day count clamps to zero like the existing price-history route");

        var clampedDays = await handler.Handle(new GetPriceDrops(99999, 10), default);
        check(clampedDays.Data.Select(row => row.InventoryId)
                .SequenceEqual(new[] { olderWindow.InventoryId, recent.InventoryId }),
            "d) The day count clamps to the same maximum as the existing price-history route");

        var loose = await handler.Handle(new GetPriceDrops(30, 0), default);
        check(!loose.Data.Any(row => row.InventoryId == tiny.InventoryId),
            "d) minPercent below one clamps to one and the 0.5% drop stays out");
        var strict = await handler.Handle(new GetPriceDrops(30, 500), default);
        check(strict.Success && strict.Message is null && strict.Data.Count == 0,
            "d) minPercent above one hundred clamps to one hundred and nothing reaches it");
    }

    // (e) El estado: última ejecución, cobertura de catálogo y frescura calculadas sobre los datos presentes.
    private static async Task StatusContract(Action<bool, string> check)
    {
        var now = DateTime.UtcNow;
        var fixture = new AutomationFixture();
        var irion = fixture.AddStore("Irion", "irion", "irion.cl", settings: """{"requestBudget":20}""");
        var luffy = fixture.AddStore("LuffyToys", "luffytoys", "luffytoys.cl", settings: """{"requestBudget":20}""");
        var closed = fixture.AddStore("Cerrada", "cerrada", "cerrada.cl", active: false);
        fixture.AddStore("Sin conector", "sin-conector", "sin-conector.cl", connectorKey: null);

        var rx = fixture.AddProduct("RG 1/144 Gundam RX-78-2");
        var zaku = fixture.AddProduct("HG 1/144 Zaku II");
        var barbatos = fixture.AddProduct("MG 1/100 Gundam Barbatos");
        var retired = fixture.AddProduct("Producto retirado");
        retired.ToggleActive();

        var fresh = fixture.AddOffer(irion, rx, 10000, true, now.AddHours(-1));
        var stale = fixture.AddOffer(luffy, rx, 9000, true, now.AddDays(-10));
        fixture.AddOffer(irion, zaku, 5000, true, now.AddDays(-3));
        // Sin fecha de observación: no es ni fresca ni vieja, y sigue siendo elegible (disponibilidad desconocida).
        fixture.AddOffer(luffy, zaku, 4000, null, null);
        fixture.AddOffer(irion, barbatos, 7000, false, now.AddHours(-1));
        var disabled = fixture.AddOffer(luffy, barbatos, 7000, true, now.AddDays(-20), active: false);
        fixture.AddOffer(closed, barbatos, 100, true, now.AddHours(-1));
        var orphan = fixture.AddOffer(irion, retired, 100, true, now.AddHours(-1));
        // Las ofertas de la tienda cerrada y del producto retirado nunca son elegibles: no cubren catálogo.
        check(!fixture.Eligible(orphan) && fixture.Eligible(fresh) && fixture.Eligible(stale) &&
            !fixture.Eligible(disabled),
            "e) The eligibility rule of the product page decides what covers the catalog");

        var older = ImportRun.Create(irion.StoreId, 20, now.AddDays(-5));
        older.RecordDiscovered(3);
        older.RecordStored(2);
        older.RecordSkipped(1);
        older.Complete(now.AddDays(-5).AddHours(1));
        var newer = ImportRun.Create(irion.StoreId, 17, now.AddDays(-1));
        newer.RecordDiscovered(4);
        newer.RecordStored(3);
        newer.RecordSkipped(1);
        newer.Complete(now.AddDays(-1).AddHours(1));
        fixture.Fake.ImportRuns.Add(older);
        fixture.Fake.ImportRuns.Add(newer);

        var handler = new GetIngestionStatusHandler(fixture.Context, new ApiResponseService());
        var response = await handler.Handle(new GetIngestionStatus(), default);

        check(response.Success && response.Message is null && response.Data.Count == 3 &&
            response.Data.Select(row => row.StoreSlug).SequenceEqual(new[] { "irion", "luffytoys", "cerrada" }),
            "e) The status lists every configured store in its configured order and nothing else");
        var first = response.Data[0];
        check(first.StoreId == irion.StoreId && first.StoreName == "Irion" && first.ConnectorKey == "shopify" &&
            first.IsActive && first.TotalProducts == 3 && first.CoveredProducts == 2 &&
            first.CoveragePercentage == 67,
            "e) Coverage is the products with at least one eligible offer over the active catalog");
        check(first.OffersObservedLast24Hours == 3 && first.OffersOlderThanSevenDays == 0,
            "e) Freshness counts every observed offer of the store, including the retired product: " +
            first.OffersObservedLast24Hours + " in 24 hours and " + first.OffersOlderThanSevenDays + " older than 7 days");
        check(first.LastRun is { } lastRun && lastRun.ImportRunId == newer.ImportRunId &&
            lastRun.StartedAtUtc == newer.StartedAtUtc && lastRun.FinishedAtUtc == newer.FinishedAtUtc &&
            lastRun.Status == (int)ImportRunStatus.Completed && lastRun.RequestBudget == 17 &&
            lastRun.DiscoveredCount == 4 && lastRun.StoredCount == 3 && lastRun.SkippedCount == 1 &&
            lastRun.FailedCount == 0,
            "e) The last run is the newest one with its status and its counters");

        var second = response.Data[1];
        check(second.LastRun is null && second.TotalProducts == 3 && second.CoveredProducts == 2 &&
            second.CoveragePercentage == 67 && second.OffersObservedLast24Hours == 0 &&
            second.OffersOlderThanSevenDays == 2,
            "e) A store never ingested shows no run and counts its stale offers: " +
            second.OffersObservedLast24Hours + "/" + second.OffersOlderThanSevenDays + " stale");

        var third = response.Data[2];
        check(!third.IsActive && third.CoveredProducts == 0 && third.CoveragePercentage == 0 &&
            third.OffersObservedLast24Hours == 1 && third.LastRun is null,
            "e) A closed store covers nothing but is still visible for the operator");

        // La regla pura se puede ejercer sin contexto: números exactos a partir de las filas recibidas.
        var metrics = IngestionMetrics.Compute("Irion", [older, newer], [], 0, now);
        check(metrics.CoveredProducts == 0 && metrics.CoveragePercentage == 0 &&
            metrics.OffersObservedLast24Hours == 0 && metrics.OffersOlderThanSevenDays == 0 &&
            metrics.LastRun is not null && metrics.LastRun.ImportRunId == newer.ImportRunId,
            "e) Empty readings describe zero coverage instead of dividing by zero");
        await Expect<System.ArgumentException>(
            () => Task.FromResult(IngestionMetrics.Compute("Irion", [], [], 0, now.ToLocalTime())),
            "La fecha de referencia del estado debe ser una fecha UTC.", "a non-UTC reference date");
    }

    // La automatización nunca tumba el host: un ciclo fallido se registra y la siguiente vuelta lo reintenta.
    private static async Task WorkerContract(Action<bool, string> check)
    {
        var runner = new CountingRunner();
        var enabled = new IngestionWorker(Scopes(runner), IngestionSettings.Parse("true", "6"),
            NullLogger<IngestionWorker>.Instance);
        await enabled.RunOnceAsync(default);
        check(runner.Calls == 1, "An enabled cycle runs the scheduled pass once");

        var off = new CountingRunner();
        var disabled = new IngestionWorker(Scopes(off), IngestionSettings.Parse(null, null),
            NullLogger<IngestionWorker>.Instance);
        await disabled.RunOnceAsync(default);
        check(off.Calls == 0, "A disabled cycle runs nothing, even if somebody invokes it");

        var failing = new CountingRunner { Failure = new InvalidOperationException("la tienda devolvió basura") };
        var resilient = new IngestionWorker(Scopes(failing), IngestionSettings.Parse("true", "6"),
            NullLogger<IngestionWorker>.Instance);
        var threw = false;
        try
        {
            await resilient.RunOnceAsync(default);
        }
        catch (Exception)
        {
            threw = true;
        }

        check(!threw && failing.Calls == 1,
            "An unhandled failure inside the pass never escapes the worker: the host keeps running");
    }

    // La composición: apagada por defecto y registrada como servicio en segundo plano.
    private static void CompositionContract(Action<bool, string> check)
    {
        using var defaults = BuildProvider();
        check(defaults.GetRequiredService<IngestionSettings>() is { Enabled: false, IntervalHours: 6 },
            "Without Ingestion configuration the automation stays off: nothing switches itself on");
        // El host puede registrar otros servicios en segundo plano (la autenticación añade el suyo): lo que se fija
        // aquí es que la ingesta programada está entre ellos exactamente una vez.
        var hosted = defaults.GetServices<IHostedService>().ToArray();
        check(hosted.Count(service => service is IngestionWorker) == 1,
            "The host registers the scheduled ingestion as a background service: " +
            string.Join(", ", hosted.Select(service => service.GetType().Name)));
        using var scope = defaults.CreateScope();
        check(scope.ServiceProvider.GetRequiredService<IIngestionRunner>() is IngestionRunner,
            "The manual trigger and the schedule share the same runner");

        using var enabled = BuildProvider(("Ingestion:Enabled", "true"), ("Ingestion:IntervalHours", "3"));
        check(enabled.GetRequiredService<IngestionSettings>() is { Enabled: true, IntervalHours: 3 },
            "The Ingestion configuration keys enable the automation and set the interval");
    }

    private static ServiceProvider BuildProvider(params (string Key, string Value)[] values)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IAuthenticationService, AnonymousAuthentication>();
        var configuration = new AutomationConfiguration
        {
            ["ConnectionStrings:BuscaTuHobby"] =
                "Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true",
            ["Jwt:Issuer"] = "checks",
            ["Jwt:Audience"] = "checks",
            ["Jwt:Key"] = new string('k', 32)
        };
        foreach (var (key, value) in values) configuration[key] = value;
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    private static IServiceScopeFactory Scopes(IIngestionRunner runner)
    {
        var services = new ServiceCollection();
        services.AddSingleton(runner);
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private static StoreContext Store(string name, string host) =>
        new(Guid.NewGuid(), name, host, "shopify", "CLP", 20, 50);

    private static PriceDropPoint Point(Guid inventoryId, int price, int daysFromNow) =>
        new(inventoryId, Guid.NewGuid(), Guid.NewGuid(), "Modelo " + price, "Irion", "irion",
            "https://irion.cl/products/modelo", price, Now.AddDays(daysFromNow));

    private static StoreProductSheet Sheet(string url, string title, string vendor, string? sku, decimal price) =>
        new(url, title, vendor, "CLP", string.Empty, null, [], [], true,
        [
            new IrionVariantPreviewDto("1", sku, null, true, price, null, null)
        ]);

    private static void Set(object target, string property, object? value) =>
        target.GetType().GetProperty(property)!.SetValue(target, value);

    private static async Task<T> Expect<T>(Func<Task> action, string message, string expectation) where T : Exception
    {
        try
        {
            await action();
        }
        catch (T exception)
        {
            // ArgumentException añade el nombre del parámetro al mensaje: el texto propio se compara como prefijo.
            var matches = exception.Message == message ||
                (typeof(T) == typeof(System.ArgumentException) &&
                 exception.Message.StartsWith(message + " (Parameter '", StringComparison.Ordinal));
            if (!matches) throw new Exception($"Unexpected message: {exception.Message}");
            return exception;
        }
        catch (Exception exception)
        {
            throw new Exception($"Unexpected exception for {expectation}: {exception.GetType().Name}: {exception.Message}");
        }

        throw new Exception("Expected " + expectation);
    }

    // Contexto en memoria más los dobles de red. El ejecutor es el real: descubre, planifica, procesa y escribe.
    private sealed class AutomationFixture
    {
        public AutomationFixture()
        {
            Context = DispatchProxy.Create<IApplicationDbContext, IngestionContext>();
            Fake = (IngestionContext)(object)Context;
            Runner = new IngestionRunner(Context, new StoreContextProvider(Context),
                new PlanningRunService(Context, Discovery), new ImportRunProcessor(Context, [Connector]),
                NullLogger<IngestionRunner>.Instance);
        }

        public IApplicationDbContext Context { get; }
        public IngestionContext Fake { get; }
        public StubDiscovery Discovery { get; } = new();
        public StubConnector Connector { get; } = new();
        public IIngestionRunner Runner { get; }

        public Store AddStore(string name, string slug, string host, string? connectorKey = "shopify",
            string? settings = null, bool active = true)
        {
            var store = Domain.Entities.Store.Create(name, "https://" + host, "bi bi-shop",
                Fake.Stores.Count, slug);
            if (connectorKey is not null) store.ConfigureConnector(connectorKey, settings);
            if (!active) store.ToggleActive();
            Fake.Stores.Add(store);
            return store;
        }

        public Product AddProduct(string name)
        {
            var product = Product.Create(name, Guid.NewGuid(), Guid.NewGuid(), null, false, string.Empty,
                string.Empty, string.Empty, default);
            Fake.Products.Add(product);
            return product;
        }

        // Una oferta con la misma regla de elegibilidad que la ficha de producto: oferta y tienda activas, producto
        // activo y disponibilidad distinta de false.
        public bool Eligible(Inventory offer) =>
            offer.IsActive && offer.Store.IsActive && offer.Product.IsActive && offer.Availability != false;

        public Inventory AddOffer(Store store, Product product, int price, bool? available, DateTime? observedAtUtc,
            bool active = true)
        {
            var offer = available is null
                ? Inventory.Create(product.ProductId, store.StoreId, price, 0, 0)
                : Inventory.CreateImportedOffer(product.ProductId, store.StoreId, price, 0, 0,
                    store.StoreWebSite.TrimEnd('/') + "/products/modelo", "1", available.Value,
                    observedAtUtc ?? Now);
            if (!active) offer.ToggleActive();
            Set(offer, nameof(Inventory.Product), product);
            Set(offer, nameof(Inventory.Store), store);
            Fake.Rows.Add(offer);
            return offer;
        }

        public void AddHistory(Inventory offer, int price, DateTime observedAtUtc)
        {
            var point = InventoryPriceHistory.Create(offer.InventoryId, price, 0, 0, offer.Availability,
                observedAtUtc);
            Set(point, nameof(InventoryPriceHistory.Inventory), offer);
            Fake.Histories.Add(point);
        }
    }

    private sealed class StubDiscovery : ICatalogDiscovery
    {
        private readonly Dictionary<string, string[]> _queued = new(StringComparer.Ordinal);

        public List<string> Hosts { get; } = [];
        public int Calls => Hosts.Count;

        public void Queue(string host, params string[] urls) => _queued[host] = urls;

        public Task<CatalogDiscoveryResult> DiscoverAsync(StoreContext store, RequestBudget budget,
            CancellationToken cancellationToken)
        {
            Hosts.Add(store.Host);
            var urls = _queued.GetValueOrDefault(store.Host) ?? [];
            return Task.FromResult(new CatalogDiscoveryResult(urls, [], [], urls.Length, urls.Length));
        }
    }

    private sealed class StubConnector : IStoreConnector
    {
        public Dictionary<string, StoreProductSheet> Sheets { get; } = new(StringComparer.Ordinal);
        public List<string> Loaded { get; } = [];

        public string Key => "shopify";

        public Task<IrionProductPreviewDto> LoadProductAsync(StoreContext store, string url,
            CancellationToken cancellationToken) => Task.FromResult(Load(url).ToPreview());

        public Task<StoreProductSheet> LoadSheetAsync(StoreContext store, string url,
            CancellationToken cancellationToken) => Task.FromResult(Load(url));

        private StoreProductSheet Load(string url)
        {
            Loaded.Add(url);
            return Sheets.TryGetValue(url, out var sheet)
                ? sheet
                : throw new InvalidOperationException("Ficha no configurada: " + url);
        }
    }

    // Arranque de ejecución sin EF: la misma regla de planificación y de idempotencia, escrita sobre las listas en
    // memoria para que el ejecutor real se pueda ejercer sin base de datos.
    private sealed class PlanningRunService(IApplicationDbContext context, ICatalogDiscovery discovery)
        : IImportRunService
    {
        public async Task<ImportRunPlan> StartAsync(StoreContext store, DateTime startedAtUtc,
            CancellationToken cancellationToken)
        {
            var budget = new RequestBudget(store.RequestBudget);
            var found = await discovery.DiscoverAsync(store, budget, cancellationToken);
            var fake = (IngestionContext)(object)context;
            var stored = new HashSet<string>(fake.ImportItems
                .Where(item => item.State == ImportItemState.Stored &&
                    fake.ImportRuns.Any(run => run.ImportRunId == item.ImportRunId &&
                        run.Status == ImportRunStatus.Completed))
                .Select(item => item.SourceUrl), StringComparer.OrdinalIgnoreCase);

            var plan = ImportRunPlanner.Start(store, budget, found, stored, startedAtUtc);
            fake.ImportRuns.Add(plan.Run);
            fake.ImportItems.AddRange(plan.Items);
            await context.SaveChangesAsync(cancellationToken);
            return plan;
        }

        public Task<IReadOnlyList<ImportItem>> ResumeAsync(Guid importRunId, CancellationToken cancellationToken) =>
            Task.FromException<IReadOnlyList<ImportItem>>(
                new NotSupportedException("La reanudación no la ejercita esta comprobación."));
    }

    private sealed class CountingRunner : IIngestionRunner
    {
        public int Calls { get; private set; }
        public Exception? Failure { get; init; }

        public Task<IngestionRunSummary> RunAsync(string storeSlug, int? requestBudget,
            CancellationToken cancellationToken) =>
            Task.FromException<IngestionRunSummary>(
                new NotSupportedException("El disparo manual no lo ejercita esta comprobación."));

        public Task<IReadOnlyList<IngestionRunSummary>> RunScheduledAsync(CancellationToken cancellationToken)
        {
            Calls++;
            return Failure is null
                ? Task.FromResult<IReadOnlyList<IngestionRunSummary>>([])
                : Task.FromException<IReadOnlyList<IngestionRunSummary>>(Failure);
        }
    }

    private sealed class AnonymousAuthentication : IAuthenticationService
    {
        public string? UserName => "checks";
        public string CreateAccessToken(string username, IList<string> roles) => "unused";
        public Task AuthenticationLOGMOD(string userName, string password) => Task.CompletedTask;
    }

    private sealed class AutomationConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string?> _values = new(StringComparer.OrdinalIgnoreCase);

        public string? this[string key]
        {
            get => _values.GetValueOrDefault(key);
            set => _values[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new NeverChanging();
        public IConfigurationSection GetSection(string key) => new AutomationSection(this, key);
    }

    private sealed class AutomationSection(AutomationConfiguration owner, string key) : IConfigurationSection
    {
        public string? this[string name]
        {
            get => owner[key + ":" + name];
            set => owner[key + ":" + name] = value;
        }

        public string Key => key;
        public string Path => key;

        public string? Value
        {
            get => owner[key];
            set => owner[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new NeverChanging();
        public IConfigurationSection GetSection(string name) => new AutomationSection(owner, key + ":" + name);
    }

    private sealed class NeverChanging : IChangeToken
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => new Nothing();

        private sealed class Nothing : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
