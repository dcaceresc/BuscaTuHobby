using System.Reflection;
using System.Text.Json;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

// Identidad de catálogo de la ingesta: la columna GTIN con su migración, la regla de emparejamiento aprobada y la
// pasada que convierte lo encolado en ofertas. Todo con dobles en memoria: ni base de datos ni red.
internal static class IngestionChecks
{
    private static readonly DateTime Start = new(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Observed = new(2026, 11, 16, 9, 30, 0, DateTimeKind.Utc);

    public static async Task Run(Action<bool, string> check)
    {
        GtinContract(check);
        ModelShape(check);
        MigrationShape(check);
        NormalizationContract(check);
        GradeAndScaleContract(check);
        DecideContract(check);
        FranchiseContract(check);
        ParkedStateContract(check);
        await ProcessorContract(check);
    }

    private static void GtinContract(Action<bool, string> check)
    {
        var withoutGtin = Product.Create("Model", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
        check(withoutGtin.Gtin is null, "A product created without a barcode keeps it as not informed");

        var withGtin = Product.Create("Model", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default,
            gtin: "6535699571882");
        check(withGtin.Gtin == "6535699571882", "The store barcode is kept on the product");
        var provided = Product.CreateWithProvidedId(Guid.NewGuid(), "Model", Guid.NewGuid(), Guid.NewGuid(), null,
            false, "", "", "", default, gtin: "4573102612345");
        check(provided.Gtin == "4573102612345", "The provided-ID factory also keeps the barcode");

        // El formulario de administración no muestra el GTIN, así que editarlo no puede borrarlo en silencio.
        withGtin.Update("Model 2", withGtin.ManufacturerId, withGtin.FranchiseId, null, false, "", "", "", default,
            withGtin.Grade, withGtin.Scale, withGtin.PartNumber);
        check(withGtin.Gtin == "6535699571882" && withGtin.ProductName == "Model 2",
            "A manual edit changes the name and preserves the ingested barcode");

        foreach (var invalid in new[] { "", " 12345678", "1234567", "123456789012345", "12345678a", "1234 5678" })
        {
            Reject(() => Product.Create("Model", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default,
                gtin: invalid), Product.InvalidGtinMessage);
            check(withoutGtin.Gtin is null, "A rejected barcode never leaves a value behind: " + invalid);
        }
    }

    private static void ModelShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var product = model.FindEntityType(typeof(Product))!;
        var gtin = product.FindProperty("Gtin");
        check(gtin is { IsNullable: true } && gtin.GetMaxLength() == 14 &&
            gtin.GetColumnType() == "nvarchar(14)",
            "The model maps the barcode as a nullable nvarchar(14)");
        check(product.GetIndexes().All(index => index.Properties.All(property => property.Name != "Gtin")),
            "GTIN carries no index: the same barcode may legitimately repeat across rows");
    }

    private static void MigrationShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var migrations = context.GetService<IMigrationsAssembly>();
        var entry = migrations.Migrations.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SingleOrDefault(pair => pair.Key.EndsWith("_ProductGtin", StringComparison.Ordinal));
        check(entry.Value is not null && entry.Key == "20261116000002_ProductGtin" &&
            string.CompareOrdinal(entry.Key, "20261116000001_ImportRuns") > 0,
            "The ProductGtin migration is discoverable and sorts after ImportRuns");
        if (entry.Value is null) return;

        var migration = migrations.CreateMigration(entry.Value, context.Database.ProviderName!);
        var up = migration.UpOperations;
        check(up.Count == 1 && up[0] is AddColumnOperation added && added.Table == "Products" &&
            added.Name == "Gtin" && added.ColumnType == "nvarchar(14)" && added.MaxLength == 14 && added.IsNullable &&
            added.DefaultValue is null && added.DefaultValueSql is null,
            "Up only adds the nullable Product.Gtin column, without a fabricated default");

        var down = migration.DownOperations;
        check(down.Count == 1 && down[0] is DropColumnOperation dropped && dropped.Table == "Products" &&
            dropped.Name == "Gtin",
            "Down drops exactly the column it created");

        var generator = context.GetService<IMigrationsSqlGenerator>();
        var sql = string.Join("\n", generator.Generate(up, model).Select(command => command.CommandText));
        check(sql.Contains("ALTER TABLE [Products] ADD [Gtin] nvarchar(14) NULL"),
            "SQL Server adds the nullable column without rebuilding the table");
        check(!sql.Contains("INDEX", StringComparison.Ordinal),
            "The generated DDL declares no index on GTIN");
        var downSql = string.Join("\n", generator.Generate(down, model).Select(command => command.CommandText));
        check(downSql.Contains("ALTER TABLE [Products] DROP COLUMN [Gtin]"),
            "SQL Server rollback drops the column");

        var initializer = context.GetService<IModelRuntimeInitializer>();
        var differ = context.GetService<IMigrationsModelDiffer>();
        var newest = migrations.Migrations.OrderBy(pair => pair.Key, StringComparer.Ordinal).Last();
        var target = initializer.Initialize(
            migrations.CreateMigration(newest.Value, context.Database.ProviderName!).TargetModel, designTime: true);
        var snapshot = initializer.Initialize(migrations.ModelSnapshot!.Model, designTime: true);
        check(!differ.HasDifferences(target.GetRelationalModel(), model.GetRelationalModel()),
            "The newest migration target matches the current model");
        check(!differ.HasDifferences(snapshot.GetRelationalModel(), model.GetRelationalModel()),
            "The snapshot matches the current model");
    }

    private static void NormalizationContract(Action<bool, string> check)
    {
        check(ProductIdentity.Normalize("[Preventa] GUNDAM EX Gundam Universe") ==
            "preventa gundam ex gundam universe",
            "Normalizing drops punctuation, brackets and case");
        check(ProductIdentity.Normalize("Gundam  Réplica") == "gundam replica",
            "Normalizing removes accents and collapses repeated spaces");
        check(ProductIdentity.Normalize("   ") == "" && ProductIdentity.Normalize(null) == "",
            "A blank title normalizes to nothing instead of inventing text");
    }

    private static void GradeAndScaleContract(Action<bool, string> check)
    {
        var grades = new (string Title, GunplaGrade Grade)[]
        {
            ("HG 1/144 Gundam Aerial", GunplaGrade.HighGrade),
            ("HGUC-191 Gundam", GunplaGrade.HighGrade),
            ("MG 1/100 Gundam Barbatos", GunplaGrade.MasterGrade),
            ("MGEX 1/100 Strike Freedom", GunplaGrade.MasterGrade),
            ("RG 1/144 Gundam RX-78-2", GunplaGrade.RealGrade),
            ("PG 1/60 Unleashed", GunplaGrade.PerfectGrade),
            ("SD Gundam Ex-Standard", GunplaGrade.SuperDeformed),
            ("[Preventa] GUNDAM EX Gundam Universe", GunplaGrade.None),
            ("Gundam Universe", GunplaGrade.None),
            ("HGUNDAM", GunplaGrade.None),
            ("", GunplaGrade.None)
        };
        foreach (var (title, grade) in grades)
            check(ProductIdentity.ParseGrade(title) == grade, "Grade parsed from title: " + title);

        var scales = new (string? Title, string? Scale)[]
        {
            ("RG 1/144 Gundam", "1/144"),
            ("RX-78 1/100", "1/100"),
            ("PG 1:60", "1/60"),
            ("Mega Size 1/48", "1/48"),
            ("1/1440 detail", null),
            ("HG 1/72", null),
            ("[Preventa] GUNDAM EX Gundam Universe", null),
            (null, null)
        };
        foreach (var (title, scale) in scales)
            check(ProductIdentity.ParseScale(title) == scale, "Scale parsed from title: " + (title ?? "null"));
    }

    private static void DecideContract(Action<bool, string> check)
    {
        var gundam = Guid.NewGuid();
        var zaku = Guid.NewGuid();
        var gundamCandidate = new ProductIdentityCandidate(gundam, "RG 1/144 Gundam RX-78-2", "Bandai",
            "RG-78-2", "4573102612345");
        var zakuCandidate = new ProductIdentityCandidate(zaku, "HG 1/144 Zaku II", "Bandai", "HG-ZAKU", null);
        var catalog = new[] { gundamCandidate, zakuCandidate };

        var created = ProductIdentity.Decide(
            new ProductIdentityInput("MG 1/100 Gundam Barbatos", "Bandai", "MG-BARB", "4573102620001"), catalog);
        check(created.Outcome == ProductIdentityOutcome.Create && created.ProductId is null && created.Reason is null,
            "Nothing matching means the product has to be created");

        var bySku = ProductIdentity.Decide(
            new ProductIdentityInput("Otro título distinto", "Otra marca", "rg-78-2", null), catalog);
        check(bySku.Outcome == ProductIdentityOutcome.Reuse && bySku.ProductId == gundam,
            "The store SKU reuses the product even when the title changed");

        var byBarcode = ProductIdentity.Decide(
            new ProductIdentityInput("Otro título distinto", "Otra marca", null, "4573102612345"), catalog);
        check(byBarcode.Outcome == ProductIdentityOutcome.Reuse && byBarcode.ProductId == gundam,
            "The barcode reuses the product when the SKU is absent");

        var byTitle = ProductIdentity.Decide(
            new ProductIdentityInput("hg 1/144  ZAKU II", "BANDAI", null, null), catalog);
        check(byTitle.Outcome == ProductIdentityOutcome.Reuse && byTitle.ProductId == zaku,
            "The normalized title plus the manufacturer name reuse the product when there is no identifier");

        var ambiguous = ProductIdentity.Decide(
            new ProductIdentityInput("Sin título conocido", "Bandai", "HG-ZAKU", null),
            [zakuCandidate, zakuCandidate with { ProductId = Guid.NewGuid() }]);
        check(ambiguous.Outcome == ProductIdentityOutcome.Park &&
            ambiguous.Reason == ProductIdentity.AmbiguousSkuReason,
            "A SKU shared by two products is parked for review instead of guessing");

        var ambiguousTitle = ProductIdentity.Decide(
            new ProductIdentityInput("HG 1/144 Zaku II", "Bandai", null, null),
            [zakuCandidate, zakuCandidate with { ProductId = Guid.NewGuid() }]);
        check(ambiguousTitle.Outcome == ProductIdentityOutcome.Park &&
            ambiguousTitle.Reason == ProductIdentity.AmbiguousTitleReason,
            "A title shared by two products of the same manufacturer is parked");

        var contradiction = ProductIdentity.Decide(
            new ProductIdentityInput("HG 1/144 Zaku II", "Bandai", "HG-OTRO", "4573102612345"), catalog);
        check(contradiction.Outcome == ProductIdentityOutcome.Park &&
            contradiction.Reason == ProductIdentity.ConflictingReferencesReason,
            "References pointing at two different products are a contradiction, not a tie-break");

        var otherSku = ProductIdentity.Decide(
            new ProductIdentityInput("HG 1/144 Zaku II", "Bandai", "HG-OTRO", null), catalog);
        check(otherSku.Outcome == ProductIdentityOutcome.Park &&
            otherSku.Reason == ProductIdentity.ConflictingSkuReason,
            "An existing product that reports another SKU is parked instead of overwriting it");

        var otherBarcode = ProductIdentity.Decide(
            new ProductIdentityInput("RG 1/144 Gundam RX-78-2", "Bandai", "RG-78-2", "4573102699999"), catalog);
        check(otherBarcode.Outcome == ProductIdentityOutcome.Park &&
            otherBarcode.Reason == ProductIdentity.ConflictingBarcodeReason,
            "An existing product that reports another barcode is parked");

        var nameTaken = ProductIdentity.Decide(
            new ProductIdentityInput("HG 1/144 Zaku II", "Otra marca", null, null), catalog);
        check(nameTaken.Outcome == ProductIdentityOutcome.Park &&
            nameTaken.Reason == ProductIdentity.NameTakenReason,
            "A name already used by another manufacturer is parked, because ProductName is unique");

        foreach (var doubtful in new[]
                 {
                     (new ProductIdentityInput(" ", "Bandai", null, null), ProductIdentity.MissingTitleReason),
                     (new ProductIdentityInput(null, "Bandai", null, null), ProductIdentity.MissingTitleReason),
                     (new ProductIdentityInput("HG 1/144 Zaku II", null, null, null),
                         ProductIdentity.MissingVendorReason),
                     (new ProductIdentityInput(new string('t', 201), "Bandai", null, null),
                         ProductIdentity.LongTitleReason),
                     (new ProductIdentityInput("HG 1/144 Zaku II", new string('v', 51), null, null),
                         ProductIdentity.LongVendorReason)
                 })
        {
            var decision = ProductIdentity.Decide(doubtful.Item1, catalog);
            check(decision.Outcome == ProductIdentityOutcome.Park && decision.Reason == doubtful.Item2,
                "Doubtful input is parked: " + doubtful.Item2);
        }
    }

    private static void FranchiseContract(Action<bool, string> check)
    {
        var gundam = new FranchiseCandidate(Guid.NewGuid(), "Gundam");
        var unicorn = new FranchiseCandidate(Guid.NewGuid(), "Gundam Unicorn");
        var franchises = new[] { gundam, unicorn };

        check(ProductIdentity.MatchFranchise("MG 1/100 Gundam Unicorn", "Bandai", franchises) == unicorn,
            "The most specific known franchise wins when two names match the title");
        check(ProductIdentity.MatchFranchise("HG 1/144 Zaku II", "Bandai", franchises) is null,
            "A title that names no known franchise maps to nothing instead of inventing one");
        check(ProductIdentity.MatchFranchise(null, "Bandai - Gundam Store", franchises) == gundam,
            "The vendor also maps to a known franchise");
        check(ProductIdentity.MatchFranchise("Gundamx", "Bandai", franchises) is null,
            "A franchise name inside a longer word does not count as a match");
    }

    private static void ParkedStateContract(Action<bool, string> check)
    {
        var item = ImportItem.Create(Guid.NewGuid(), "https://irion.cl/products/a");
        item.MarkParked(ProductIdentity.MissingTitleReason);
        check(item.State == ImportItemState.Parked && item.ErrorMessage == ProductIdentity.MissingTitleReason &&
            item.HttpStatus is null && item.AttemptedAtUtc is null && !item.IsProcessable &&
            !item.IsTransientFailure,
            "A parked item keeps its reason, publishes no HTTP status and is never retried on resume");
        Reject(() => item.MarkParked("  "), "El mensaje de error del elemento no es válido.");
        check(item.State == ImportItemState.Parked && item.ErrorMessage == ProductIdentity.MissingTitleReason,
            "A rejected park reason leaves the item untouched");
    }

    private static async Task ProcessorContract(Action<bool, string> check)
    {
        await SkuMatchReusesProduct(check);
        await BarcodeMatchReusesProduct(check);
        await UnmatchedItemCreatesCatalog(check);
        await DoubtfulItemIsParked(check);
        await SheetWithoutIdentifiersIsStored(check);
        await SecondPassIsIdempotent(check);
        await BudgetAndPolitenessStopThePass(check);
        await CancellationDoesNotWrite(check);
        await Rejections(check);
    }

    // (a) El SKU de la tienda reutiliza el producto y actualiza la oferta, con un solo punto de historial.
    private static async Task SkuMatchReusesProduct(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var product = fixture.AddProduct("RG 1/144 Gundam RX-78-2", "Bandai", GunplaGrade.RealGrade, "1/144",
            "RG-78-2", null);
        var offer = fixture.AddOffer(product, 10000, 0, 0, "https://irion.cl/products/rx-78-2", "9001", true);
        var run = fixture.Enqueue("https://irion.cl/products/rx-78-2");
        fixture.Connector.Sheets["https://irion.cl/products/rx-78-2"] =
            Sheet("https://irion.cl/products/rx-78-2", "RG 1/144 Gundam RX-78-2", "Bandai", "RG-78-2", null,
                12000, 15000, available: false);

        var result = await fixture.Process(run.Run);
        check(result.Stored == 1 && result.Parked == 0 && result.Failed == 0 && result.Completed &&
            run.Run.Status == ImportRunStatus.Completed && run.Run.StoredCount == 1,
            "a) The matched item is stored once and the run completes");
        check(fixture.Fake.Products.Count == 1 && fixture.Fake.Rows.Count == 1,
            "a) A SKU match never creates a second product or a second offer");
        var row = fixture.Fake.Rows.Single();
        check(row.InventoryId == offer.InventoryId && row.Price == 12000 && row.OriginalPrice == 15000 &&
            row.DiscountPercentage == 20 && row.Availability == false && row.ObservedAtUtc == Observed &&
            row.SourceUrl == "https://irion.cl/products/rx-78-2" && row.ExternalVariantId == "9001",
            "a) The offer is updated with price, original price, discount, availability and freshness");
        check(fixture.Fake.Histories.Count == 1 && !fixture.Fake.Histories[0].IsBaseline &&
            fixture.Fake.Histories[0].Price == 12000 && fixture.Fake.Histories[0].Availability == false,
            "a) The change-only recorder appends exactly one point for the changed observation");
        check(product.ProductName == "RG 1/144 Gundam RX-78-2" && product.PartNumber == "RG-78-2" &&
            product.Gtin is null && product.ManufacturerId == fixture.ManufacturerNamed("Bandai").ManufacturerId,
            "a) Reusing a product never rewrites its identity data");
        check(run.Items.Single().State == ImportItemState.Stored &&
            run.Items.Single().AttemptedAtUtc == Observed && run.Items.Single().HttpStatus == 200,
            "a) The stored item records its status and attempt time");
    }

    // (b) Sin SKU, el código de barras reutiliza el producto y crea su oferta en la tienda.
    private static async Task BarcodeMatchReusesProduct(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var product = fixture.AddProduct("Gundam EX", "Bandai - Tamashii Nations", GunplaGrade.None, null,
            null, "6535699571882");
        const string url = "https://irion.cl/products/gundam-ex";
        fixture.Connector.Sheets[url] = Sheet(url, "[Preventa] GUNDAM EX Gundam Universe",
            "Bandai - Tamashii Nations", null, "6535699571882", 31990, 39990);
        var run = fixture.Enqueue(url);

        var result = await fixture.Process(run.Run);

        check(result.Stored == 1 && fixture.Fake.Products.Count == 1 && fixture.Fake.Rows.Count == 1,
            "b) A barcode match reuses the product and creates exactly one offer");
        var row = fixture.Fake.Rows.Single();
        check(row.ProductId == product.ProductId && row.Price == 31990 && row.OriginalPrice == 39990 &&
            row.DiscountPercentage == 20 && row.Availability == true && row.Condition == ProductCondition.New &&
            row.PaymentMethodLabel is null,
            "b) The offer carries the store prices, availability and the honest default condition");
        check(product.Gtin == "6535699571882" && product.PartNumber is null &&
            product.ProductName == "Gundam EX",
            "b) The barcode match does not rename or re-identify the reused product");
    }

    // (c) Sin coincidencias se crea fabricante, producto, oferta y GTIN, y la franquicia conocida se reconoce.
    private static async Task UnmatchedItemCreatesCatalog(Action<bool, string> check)
    {
        var fixture = new Fixture();
        const string unknown = "https://irion.cl/products/zaku";
        const string known = "https://irion.cl/products/barbatos";
        fixture.Connector.Sheets[unknown] = Sheet(unknown, "RG 1/144 Zaku II", "Bandai", "RG-ZAKU",
            "4573102612345", 45990, description: "  Maqueta nueva  ");
        fixture.Connector.Sheets[known] = Sheet(known, "MG 1/100 Gundam Barbatos", "Bandai", "MG-BARB", null,
            67990);
        var run = fixture.Enqueue(unknown, known);

        var result = await fixture.Process(run.Run);

        check(result.Stored == 2 && result.Parked == 0 && result.Completed &&
            fixture.Fake.Products.Count == 2 && fixture.Fake.Rows.Count == 2,
            "c) Two unmatched sheets create two products and two offers in one pass");
        check(fixture.Fake.Manufacturers.Count == 1 && fixture.Fake.Manufacturers[0].ManufacturerName == "Bandai",
            "c) The manufacturer comes from the store vendor and is created only once");

        var zaku = fixture.Fake.Products.Single(row => row.PartNumber == "RG-ZAKU");
        check(zaku.ProductName == "RG 1/144 Zaku II" && zaku.Grade == GunplaGrade.RealGrade &&
            zaku.Scale == "1/144" && zaku.Gtin == "4573102612345" && zaku.PartNumber == "RG-ZAKU" &&
            zaku.IsActive && zaku.ProductDescription == "Maqueta nueva",
            "c) The created product keeps the store SKU, the barcode and the parsed grade and scale");
        var offer = fixture.Fake.Rows.Single(row => row.ProductId == zaku.ProductId);
        check(offer.Price == 45990 && offer.OriginalPrice == 0 && offer.DiscountPercentage == 0 &&
            offer.Availability == true && offer.ObservedAtUtc == Observed && offer.SourceUrl == unknown &&
            offer.ExternalVariantId == "9001" && offer.IsActive,
            "c) The created offer carries the fetched prices, availability and freshness");

        var unclassified = fixture.Fake.Franchises.SingleOrDefault(row =>
            row.FranchiseName == ProductIdentity.UnclassifiedFranchiseName);
        check(unclassified is not null && zaku.FranchiseId == unclassified.FranchiseId,
            "c) A title with no known franchise gets the single shared Sin clasificar franchise");
        var barbatos = fixture.Fake.Products.Single(row => row.PartNumber == "MG-BARB");
        check(barbatos.FranchiseId == fixture.Gundam.FranchiseId &&
            barbatos.Grade == GunplaGrade.MasterGrade && barbatos.Scale == "1/100" &&
            barbatos.ManufacturerId == zaku.ManufacturerId,
            "c) A title that names a known franchise reuses it and the created manufacturer");
    }

    // Un defecto que solo apareció en la primera pasada real: LuffyToys publica fichas sin SKU ni código de barras, y
    // el dominio rechaza un número de parte vacío. El vacío del conector es "sin dato" y la ficha se guarda igual.
    private static async Task SheetWithoutIdentifiersIsStored(Action<bool, string> check)
    {
        var fixture = new Fixture();
        const string bare = "https://luffytoys.cl/products/gundam-ex";
        fixture.Connector.Sheets[bare] = Sheet(bare, "[Preventa] GUNDAM EX Gundam Universe",
            "Bandai - Tamashii Nations", "", "", 31990, original: 39990);
        var run = fixture.Enqueue(bare);

        var result = await fixture.Process(run.Run);

        check(result.Stored == 1 && result.Parked == 0 && result.Failed == 0 && result.Completed,
            "g) A sheet with an empty SKU and barcode is stored instead of aborting the pass");
        var product = fixture.Fake.Products.Single();
        check(product.PartNumber is null && product.Gtin is null && fixture.Fake.Rows.Single().Price == 31990,
            "g) Missing identifiers are stored as no data, never as an empty string");
    }

    // (e) Lo dudoso se aparca con motivo y no crea ni publica nada.
    private static async Task DoubtfulItemIsParked(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var duplicated = fixture.AddProduct("HG 1/144 Zaku II", "Bandai", GunplaGrade.HighGrade, "1/144",
            "DUP-1", null);
        fixture.AddProduct("HG 1/144 Zaku II (segunda tienda)", "Bandai", GunplaGrade.HighGrade, "1/144",
            "DUP-1", null);
        var conflicting = fixture.AddProduct("Gundam Conflicto", "Bandai", GunplaGrade.None, null, "CONF-1",
            "2222222222222");

        const string withoutTitle = "https://irion.cl/products/sin-titulo";
        const string withoutPrice = "https://irion.cl/products/sin-precio";
        const string ambiguous = "https://irion.cl/products/ambiguo";
        const string contradiction = "https://irion.cl/products/conflicto";
        fixture.Connector.Sheets[withoutTitle] = Sheet(withoutTitle, "", "Bandai", "X-1", null, 1000);
        fixture.Connector.Sheets[withoutPrice] = Sheet(withoutPrice, "RG 1/144 Zaku II", "Bandai", "NUEVO",
            null, 0);
        fixture.Connector.Sheets[ambiguous] = Sheet(ambiguous, "Otro título", "Otra marca", "DUP-1", null, 1000);
        fixture.Connector.Sheets[contradiction] = Sheet(contradiction, "Gundam Conflicto", "Bandai", "CONF-1",
            "3333333333333", 1000);
        var run = fixture.Enqueue(withoutTitle, withoutPrice, ambiguous, contradiction);

        var result = await fixture.Process(run.Run);

        check(result.Stored == 0 && result.Parked == 4 && result.Completed && run.Run.StoredCount == 0 &&
            run.Run.SkippedCount == 4,
            "e) Four doubtful sheets are parked and none of them is published");
        check(fixture.Fake.Products.Count == 3 && fixture.Fake.Rows.Count == 0 &&
            fixture.Fake.Histories.Count == 0 && fixture.Fake.Products.All(row => row.IsActive),
            "e) Parking creates no product, no offer, no history row and no inactive product");
        check(fixture.Fake.Products.Count(row => row.PartNumber == "DUP-1") == 2 &&
            fixture.Fake.Products.Contains(duplicated) && fixture.Fake.Products.Contains(conflicting),
            "e) The existing catalog is left exactly as it was");

        var expected = new Dictionary<string, string>
        {
            [withoutTitle] = ProductIdentity.MissingTitleReason,
            [withoutPrice] = ProductIdentity.MissingPriceReason,
            [ambiguous] = ProductIdentity.AmbiguousSkuReason,
            [contradiction] = ProductIdentity.ConflictingBarcodeReason
        };
        foreach (var (url, reason) in expected)
        {
            var item = run.Items.Single(row => row.SourceUrl == url);
            check(item.State == ImportItemState.Parked && item.ErrorMessage == reason &&
                item.HttpStatus is null && item.AttemptedAtUtc is null && !item.IsProcessable,
                "e) The parked item keeps its exact review reason: " + url);
        }

        // Un aparcado no se reintenta solo: la ejecución completada ni siquiera se puede volver a procesar.
        var saves = fixture.Fake.Saves;
        var before = fixture.Fake.Snapshot;
        await Expect<InvalidOperationException>(
            () => fixture.Process(run.Run),
            "Una ejecución completada no se reanuda: crea una ejecución nueva.",
            "e) Reprocessing a completed run with parked items is refused");
        check(fixture.Fake.Saves == saves && fixture.Fake.Snapshot == before && fixture.Fake.Rows.Count == 0,
            "e) The refused reprocessing writes nothing");
    }

    // (f) Una segunda pasada sobre las mismas fichas no duplica nada ni infla el historial.
    private static async Task SecondPassIsIdempotent(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var product = fixture.AddProduct("RG 1/144 Gundam RX-78-2", "Bandai", GunplaGrade.RealGrade, "1/144",
            "RG-78-2", null);
        fixture.AddOffer(product, 12000, 15000, 20, "https://irion.cl/products/rx-78-2", "9001", false);
        const string existing = "https://irion.cl/products/rx-78-2";
        const string fresh = "https://irion.cl/products/zaku";
        fixture.Connector.Sheets[existing] = Sheet(existing, "RG 1/144 Gundam RX-78-2", "Bandai", "RG-78-2",
            null, 12000, 15000, available: false);
        fixture.Connector.Sheets[fresh] = Sheet(fresh, "RG 1/144 Zaku II", "Bandai", "RG-ZAKU",
            "4573102612345", 45990);

        var first = fixture.Enqueue(existing, fresh);
        var firstResult = await fixture.Process(first.Run);
        var products = fixture.Fake.Products.Count;
        var offers = fixture.Fake.Rows.Count;
        var history = fixture.Fake.Histories.Count;
        check(firstResult.Stored == 2 && products == 2 && offers == 2 && history == 0,
            "f) The first pass stores both sheets and the identical observation appends no history row");

        // Segunda ejecución completa sobre las mismas URL: el catálogo se reconoce por SKU y por título.
        var second = fixture.Enqueue(existing, fresh);
        var secondResult = await fixture.Process(second.Run);

        check(secondResult.Stored == 2 && secondResult.Completed &&
            fixture.Fake.Products.Count == products && fixture.Fake.Rows.Count == offers,
            "f) A second pass creates no duplicate product or offer");
        check(fixture.Fake.Histories.Count == history,
            "f) A second pass with unchanged prices writes no price-history row");
        check(fixture.Fake.Rows.GroupBy(row => (row.StoreId, row.ProductId)).All(group => group.Count() == 1),
            "f) The unique (store, product) pair holds after two passes");
        var created = fixture.Fake.Products.Single(row => row.PartNumber == "RG-ZAKU");
        check(fixture.Fake.Rows.Single(row => row.ProductId == created.ProductId).ObservedAtUtc == Observed,
            "f) The second pass refreshes freshness without creating a new row");
        check(second.Items.All(item => item.State == ImportItemState.Stored),
            "f) The second run audits its items as stored by itself");
    }

    // El presupuesto y la cortesía con la tienda cortan la pasada sin cerrar la ejecución.
    private static async Task BudgetAndPolitenessStopThePass(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var urls = new[]
        {
            "https://irion.cl/products/a-uno", "https://irion.cl/products/b-dos", "https://irion.cl/products/c-tres"
        };
        foreach (var url in urls)
            fixture.Connector.Sheets[url] = Sheet(url, "HG 1/144 Modelo " + url[^3..], "Bandai", url[^3..], null,
                1000);
        var budgeted = fixture.Enqueue(2, urls);
        var result = await fixture.Process(budgeted.Run);

        check(result.Stored == 2 && result.Remaining == 1 && result.BudgetExhausted && !result.Completed &&
            fixture.Connector.Loaded.Count == 2,
            "The run budget caps the number of fetched sheets");
        check(budgeted.Run.Status == ImportRunStatus.Running && budgeted.Run.FinishedAtUtc is null &&
            budgeted.Run.StoredCount == 2 &&
            budgeted.Items.Single(item => item.SourceUrl == urls[2]).State == ImportItemState.Pending,
            "An exhausted budget leaves the run open and the pending item for a resume");

        var polite = new Fixture();
        foreach (var url in urls)
            polite.Connector.Sheets[url] = Sheet(url, "HG 1/144 Modelo " + url[^3..], "Bandai", url[^3..], null,
                1000);
        polite.Connector.Failures[urls[1]] = new Application.Common.Exceptions.StoreRateLimitException(
            "Irion", "Espera antes de reintentar.");
        var limited = polite.Enqueue(urls);
        var limitedResult = await polite.Process(limited.Run);
        check(limitedResult.Stored == 1 && limitedResult.Failed == 1 && limitedResult.Remaining == 1 &&
            !limitedResult.Completed && !limitedResult.BudgetExhausted &&
            polite.Connector.Loaded.Count == 2,
            "A 429 from the store stops the pass after recording the transient failure");
        var limitedItem = limited.Items.Single(item => item.SourceUrl == urls[1]);
        check(limitedItem.State == ImportItemState.Failed && limitedItem.HttpStatus == 429 &&
            limitedItem.IsTransientFailure && limitedItem.IsProcessable,
            "The rate-limited item is audited as a retryable 429");
        check(limited.Items.Single(item => item.SourceUrl == urls[2]).State == ImportItemState.Pending,
            "The item after the 429 is not fetched and stays pending");

        var robots = new Fixture();
        robots.Connector.Failures[urls[0]] = new Application.Common.Exceptions.StorePathDisallowedException(
            "Irion", "irion.cl", "/products/uno.js");
        robots.Connector.Sheets[urls[1]] = Sheet(urls[1], "HG 1/144 Modelo b-dos", "Bandai", "B-DOS", null, 1000);
        robots.Connector.Failures[urls[2]] = new IOException("connection reset");
        var guarded = robots.Enqueue(urls);
        var guardedResult = await robots.Process(guarded.Run);
        check(guardedResult.Skipped == 1 && guardedResult.Stored == 1 && guardedResult.Failed == 1 &&
            guardedResult.Completed,
            "A robots refusal is audited as a skip while the pass continues");
        var refused = guarded.Items.Single(item => item.SourceUrl == urls[0]);
        check(refused.State == ImportItemState.Skipped && refused.HttpStatus is null &&
            refused.ErrorMessage!.StartsWith("robots.txt", StringComparison.Ordinal),
            "The robots refusal keeps the reason and publishes no HTTP status");
        check(robots.Connector.Loaded.Count == 3,
            "The connector is still called for the remaining sheets of the pass");
    }

    private static async Task CancellationDoesNotWrite(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var urls = new[] { "https://irion.cl/products/a-uno", "https://irion.cl/products/b-dos" };
        foreach (var url in urls)
            fixture.Connector.Sheets[url] = Sheet(url, "HG 1/144 Modelo " + url[^3..], "Bandai", url[^3..], null,
                1000);
        var run = fixture.Enqueue(urls);
        var before = fixture.Fake.Snapshot;
        using var cancellation = new CancellationTokenSource();
        fixture.Connector.OnLoad = _ => cancellation.Cancel();

        await Expect<OperationCanceledException>(
            () => fixture.Process(run.Run, cancellation.Token), null,
            "A cancelled pass stops cleanly instead of finishing the run");

        check(fixture.Fake.Saves == 0 && fixture.Fake.Snapshot == before,
            "Candidate state after cancellation equals the last stored state: nothing was written");
        check(run.Run.Status == ImportRunStatus.Running && run.Run.FinishedAtUtc is null &&
            run.Run.StoredCount == 0,
            "A cancelled run is neither completed nor counted");
    }

    private static async Task Rejections(Action<bool, string> check)
    {
        var fixture = new Fixture();
        var run = fixture.Enqueue("https://irion.cl/products/a-uno");
        var unknownRun = Guid.NewGuid();
        await Expect<Application.Common.Exceptions.NotFoundException>(
            () => new ImportRunProcessor(fixture.Context, [fixture.Connector])
                .ProcessAsync(fixture.StoreContext, unknownRun, Observed, default),
            $"La ejecución {unknownRun} no existe.",
            "An unknown run is rejected explicitly");

        var otherStore = new StoreContext(Guid.NewGuid(), "Otra", "otra.cl", "shopify", "CLP", 20, 50);
        await Expect<System.ArgumentException>(
            () => new ImportRunProcessor(fixture.Context, [fixture.Connector])
                .ProcessAsync(otherStore, run.Run.ImportRunId, Observed, default),
            "La ejecución no pertenece a la tienda indicada.",
            "A run of another store is rejected instead of writing into the wrong store");

        var unknownConnector = new StoreContext(fixture.Store.StoreId, fixture.Store.StoreName, "irion.cl",
            "json-ld", "CLP", 20, 50);
        await Expect<Application.Common.Exceptions.StoreConfigurationException>(
            () => new ImportRunProcessor(fixture.Context, [fixture.Connector])
                .ProcessAsync(unknownConnector, run.Run.ImportRunId, Observed, default),
            "La tienda Irion declara un conector desconocido: json-ld.",
            "A store that declares an unregistered connector fails explicitly");

        await Expect<System.ArgumentException>(
            () => fixture.Process(run.Run, Observed.ToLocalTime()),
            "La fecha de la observación debe ser una fecha UTC.",
            "A non-UTC observation time is rejected before touching the run");
        check(run.Run.StoredCount == 0 && fixture.Connector.Loaded.Count == 0,
            "Rejected calls never fetch or write anything");
    }

    private static IrionProductPreviewDto Sheet(string url, string title, string vendor, string? sku,
        string? barcode, decimal price, decimal? original = null, bool available = true, string description = "",
        IReadOnlyList<string>? images = null) =>
        new(url, title, vendor, "CLP", description, [], images ?? [], available,
        [
            new IrionVariantPreviewDto("9001", sku, barcode, available, price, original,
                original > price ? Math.Round((original.Value - price) / original.Value * 100m, 2) : null)
        ]);

    private static ApplicationDbContext StructuralContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true")
            .Options);

    private static void Reject(Action action, string message)
    {
        try
        {
            action();
        }
        catch (System.ArgumentException exception)
        {
            var text = exception.Message;
            var parameter = text.IndexOf(" (Parameter '", StringComparison.Ordinal);
            if (parameter > 0) text = text[..parameter];
            if (text == message) return;
            throw new Exception($"Unexpected rejection message: {text}");
        }

        throw new Exception("Expected rejection: " + message);
    }

    private static async Task Expect<T>(Func<Task> action, string? message, string expectation)
        where T : Exception
    {
        try
        {
            await action();
        }
        catch (T exception)
        {
            // ArgumentException añade el nombre del parámetro al mensaje; el texto propio se compara como prefijo.
            if (message is not null && !exception.Message.StartsWith(message, StringComparison.Ordinal))
                throw new Exception($"Unexpected message: {exception.Message}");
            return;
        }
        catch (Exception exception)
        {
            throw new Exception($"Unexpected exception for {expectation}: {exception.GetType().Name}");
        }

        throw new Exception("Expected " + expectation);
    }

    private sealed class Fixture
    {
        public Fixture()
        {
            Context = DispatchProxy.Create<IApplicationDbContext, IngestionContext>();
            Fake = (IngestionContext)(object)Context;
            Store = Store.Create("Irion", "https://irion.cl", "bi bi-shop", 0, "irion");
            Fake.Stores.Add(Store);
            StoreContext = new StoreContext(Store.StoreId, Store.StoreName, "irion.cl", "shopify", "CLP", 20, 50);
            Gundam = Franchise.Create("Gundam");
            Fake.Franchises.Add(Gundam);
        }

        public IApplicationDbContext Context { get; }
        public IngestionContext Fake { get; }
        public Store Store { get; }
        public StoreContext StoreContext { get; }
        public Franchise Gundam { get; }
        public StubConnector Connector { get; } = new();

        public Manufacturer ManufacturerNamed(string name)
        {
            var existing = Fake.Manufacturers.FirstOrDefault(row => row.ManufacturerName == name);
            if (existing is not null) return existing;
            var created = Manufacturer.Create(name);
            Fake.Manufacturers.Add(created);
            return created;
        }

        public Product AddProduct(string name, string manufacturer, GunplaGrade grade, string? scale,
            string? partNumber, string? gtin)
        {
            var product = Product.Create(name, ManufacturerNamed(manufacturer).ManufacturerId, Gundam.FranchiseId,
                null, false, "", "", "", default, grade, scale, partNumber, gtin);
            Fake.Products.Add(product);
            return product;
        }

        public Inventory AddOffer(Product product, int price, int original, int discount, string sourceUrl,
            string variant, bool available)
        {
            var row = Inventory.CreateImportedOffer(product.ProductId, Store.StoreId, price, original, discount,
                sourceUrl, variant, available, Start);
            Fake.Rows.Add(row);
            return row;
        }

        public RunPlan Enqueue(params string[] urls) => Enqueue(Math.Max(urls.Length + 1, 2), urls);

        public RunPlan Enqueue(int budget, params string[] urls)
        {
            var discovery = new CatalogDiscoveryResult(urls, [], [], urls.Length, urls.Length);
            var plan = ImportRunPlanner.Start(StoreContext, new RequestBudget(budget), discovery,
                new HashSet<string>(), Start);
            Fake.ImportRuns.Add(plan.Run);
            Fake.ImportItems.AddRange(plan.Items);
            Fake.Capture();
            return new RunPlan(plan.Run, plan.Items);
        }

        public Task<ImportRunProcessingResult> Process(ImportRun run,
            CancellationToken cancellationToken = default) => Process(run, Observed, cancellationToken);

        public Task<ImportRunProcessingResult> Process(ImportRun run, DateTime observedAtUtc,
            CancellationToken cancellationToken = default) =>
            new ImportRunProcessor(Context, [Connector])
                .ProcessAsync(StoreContext, run.ImportRunId, observedAtUtc, cancellationToken);
    }

    private sealed record RunPlan(ImportRun Run, IReadOnlyList<ImportItem> Items);

    private sealed class StubConnector : IStoreConnector
    {
        public Dictionary<string, IrionProductPreviewDto> Sheets { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, Exception> Failures { get; } = new(StringComparer.Ordinal);
        public List<string> Loaded { get; } = [];
        public Action<string>? OnLoad { get; set; }

        public string Key => "shopify";

        public Task<IrionProductPreviewDto> LoadProductAsync(
            StoreContext store, string url, CancellationToken cancellationToken)
        {
            Loaded.Add(url);
            OnLoad?.Invoke(url);
            cancellationToken.ThrowIfCancellationRequested();
            if (Failures.TryGetValue(url, out var failure)) throw failure;
            return Task.FromResult(Sheets[url]);
        }
    }
}

// Foto del catálogo y de la ejecución tal como quedaría en la base: sólo se toma al guardar, para que una
// cancelación a mitad se pueda comparar contra lo último realmente almacenado.
public class IngestionContext : InspectionContext
{
    public List<ImportRun> ImportRuns { get; } = [];
    public List<ImportItem> ImportItems { get; } = [];
    public List<Manufacturer> Manufacturers { get; } = [];
    public List<Franchise> Franchises { get; } = [];
    public string Snapshot { get; private set; } = "";

    public void Capture() => Snapshot = JsonSerializer.Serialize(new
    {
        Runs = ImportRuns.Select(run => new
        {
            run.ImportRunId, run.StoreId, run.Status, run.FinishedAtUtc, run.DiscoveredCount,
            run.StoredCount, run.SkippedCount, run.FailedCount
        }),
        Items = ImportItems.OrderBy(item => item.SourceUrl, StringComparer.Ordinal).Select(item => new
        {
            item.SourceUrl, item.State, item.HttpStatus, item.ErrorMessage, item.AttemptedAtUtc
        }),
        Products = Products.Select(row => new
        {
            row.ProductId, row.ProductName, row.ManufacturerId, row.FranchiseId, row.PartNumber, row.Gtin,
            row.Grade, row.Scale, row.ProductDescription, row.IsActive
        }),
        Offers = Rows.Select(row => new
        {
            row.InventoryId, row.StoreId, row.ProductId, row.Price, row.OriginalPrice,
            row.DiscountPercentage, row.Availability, row.SourceUrl, row.ExternalVariantId, row.Condition,
            row.PaymentMethodLabel, row.ObservedAtUtc, row.IsActive
        }),
        Histories = Histories.Select(row => new
        {
            row.InventoryId, row.Price, row.OriginalPrice, row.DiscountPercentage, row.Availability,
            row.ObservedAtUtc, row.IsBaseline
        }),
        Manufacturers = Manufacturers.Select(row => new { row.ManufacturerId, row.ManufacturerName }),
        Franchises = Franchises.Select(row => new { row.FranchiseId, row.FranchiseName })
    });

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        void Query(CancellationToken token)
        {
            QueryHook?.Invoke(token);
            token.ThrowIfCancellationRequested();
            if (QueryError is not null) throw QueryError;
        }

        if (method!.Name == "SaveChangesAsync")
        {
            var result = base.Invoke(method, args);
            Capture();
            return result;
        }

        return method.Name switch
        {
            "get_ImportRuns" => new InspectionSet<ImportRun>(ImportRuns, Query),
            "get_ImportItems" => new InspectionSet<ImportItem>(ImportItems, Query),
            "get_Manufacturers" => new InspectionSet<Manufacturer>(Manufacturers, Query),
            "get_Franchises" => new InspectionSet<Franchise>(Franchises, Query),
            _ => base.Invoke(method, args)
        };
    }
}
