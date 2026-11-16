using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Commands.ConfirmIrionOffer;
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.ToggleInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Application.Maintainer.Inventories.IrionOffers;
using Application.Maintainer.Inventories.Queries.InspectIrionOffer;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

internal static class PriceHistoryChecks
{
    private const string Source = "https://irion.cl/products/model-1";

    public static async Task Run(Action<bool, string> check)
    {
        // (a)-(c) Registro solo ante cambios a través de los handlers manuales.
        var context = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
        var fake = (ContextProxy)(object)context;
        var product = Guid.NewGuid();
        var store = Guid.NewGuid();
        var services = new ApiResponseService();
        var response = await new CreateInventoryHandler(context, services).Handle(new(product, store, 100, 0, 0), default);
        var row = fake.Rows.Single();
        var baseline = fake.Histories.SingleOrDefault();
        check(response.Success && response.Message == "Inventario creado correctamente" && fake.Saves == 1 && fake.Histories.Count == 1,
            "Creating an offer still succeeds with the same message and appends exactly one history point");
        check(baseline is not null && baseline.InventoryId == row.InventoryId && baseline.Price == 100 &&
            baseline.OriginalPrice == 0 && baseline.DiscountPercentage == 0 && baseline.IsBaseline &&
            baseline.Availability == row.Availability,
            "The creation point is a baseline carrying the offer's initial price tuple");
        var started = DateTime.UtcNow;
        string Rows() => JsonSerializer.Serialize(fake.Rows);

        var beforeUpdate = Rows();
        response = await new UpdateInventoryHandler(context, services).Handle(new()
        {
            InventoryId = row.InventoryId, ProductId = product, StoreId = store,
            Price = 150, OriginalPrice = 180, DiscountPercentage = 17
        }, default);
        var point = fake.Histories.LastOrDefault();
        check(response.Success && response.Message == "Inventario actualizado correctamente" && fake.Saves == 2 &&
            fake.Rows.Count == 1 && Rows() != beforeUpdate && fake.Histories.Count == 2,
            "(a) A price change appends exactly one history row");
        check(point is not null && point.InventoryId == row.InventoryId && point.Price == 150 &&
            point.OriginalPrice == 180 && point.DiscountPercentage == 17 && point.Availability == row.Availability &&
            !point.IsBaseline && point.ObservedAtUtc >= started && point.ObservedAtUtc <= DateTime.UtcNow &&
            point.ObservedAtUtc.Kind == DateTimeKind.Utc,
            "(a) The appended point carries post-change values, no baseline flag and the observation instant");

        // (b) Rutas que no son solo de precio: activación administrativa y ediciones manuales idénticas.
        var beforeToggle = Rows();
        response = await new ToggleInventoryHandler(context, services).Handle(new(row.InventoryId), default);
        check(response.Success && response.Message == "Inventario actualizado correctamente" && !row.IsActive &&
            fake.Histories.Count == 2 && fake.Saves == 3 && Rows() != beforeToggle,
            "(b) Toggling activation changes no price field and appends no history row");
        var beforeIdentical = Rows();
        response = await new UpdateInventoryHandler(context, services).Handle(new()
        {
            InventoryId = row.InventoryId, ProductId = product, StoreId = store,
            Price = 150, OriginalPrice = 180, DiscountPercentage = 17
        }, default);
        check(response.Success && response.Message == "Inventario actualizado correctamente" &&
            Rows() == beforeIdentical && fake.Histories.Count == 2 && fake.Saves == 4,
            "(b) Repeating the identical price tuple appends no history row");

        // (c) Solo difiere el descuento.
        response = await new UpdateInventoryHandler(context, services).Handle(new()
        {
            InventoryId = row.InventoryId, ProductId = product, StoreId = store,
            Price = 150, OriginalPrice = 180, DiscountPercentage = 25
        }, default);
        check(response.Success && response.Message == "Inventario actualizado correctamente" && fake.Histories.Count == 3 &&
            fake.Histories[2].Price == 150 && fake.Histories[2].OriginalPrice == 180 &&
            fake.Histories[2].DiscountPercentage == 25 && !fake.Histories[2].IsBaseline,
            "(c) Changing only DiscountPercentage appends exactly one history row");

        // La misma regla protege las rutas de importación, que también mutan Availability.
        var importContext = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var import = (InspectionContext)(object)importContext;
        var importedProduct = Product.Create("Model", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
        var irion = Store.Create("Irion", "https://irion.cl/", "", 0, "irion");
        import.Products.Add(importedProduct);
        import.Stores.Add(irion);
        var observed = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
        var linked = Inventory.CreateImportedOffer(importedProduct.ProductId, irion.StoreId, 100, 0, 0, Source, "123", true, observed);
        typeof(Inventory).GetProperty(nameof(Inventory.RowVersion))!.SetValue(linked, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        import.Rows.Add(linked);
        var extractor = new Extractor(available: true, price: 90, original: 100);
        var confirmation = new ConfirmIrionOfferHandler(importContext, new ApiResponseService(), extractor,
            new InspectIrionOfferHandler(importContext, new ApiResponseService()));
        ConfirmIrionOffer Request(bool available) => new(importedProduct.ProductId, irion.StoreId, Source, "123",
            linked.InventoryId, Convert.ToBase64String(linked.RowVersion), new IrionOfferSnapshot("CLP", 90, 100, available));

        var saved = await confirmation.Handle(Request(true), default);
        check(saved.Success && saved.Data?.Outcome == "Saved" && import.Histories.Count == 1 &&
            import.Histories[0].Price == 90 && import.Histories[0].Availability == true &&
            !import.Histories[0].IsBaseline && import.Histories[0].ObservedAtUtc == linked.ObservedAtUtc,
            "(a) An offer import that lowers the price appends one point at the inventory observation instant");
        var repeated = await confirmation.Handle(Request(true), default);
        check(repeated.Success && repeated.Data?.Outcome == "AlreadySaved" && import.Histories.Count == 1,
            "(b) A repeated identical import appends no history row");
        extractor.Available = false;
        var availabilityOnly = await confirmation.Handle(Request(false), default);
        check(availabilityOnly.Success && availabilityOnly.Data?.Outcome == "Saved" && import.Histories.Count == 2 &&
            import.Histories[1].Price == 90 && import.Histories[1].Availability == false,
            "(c) An import that changes only Availability appends exactly one history row");

        // Una creación por la ruta de importación también inicia la serie con el punto de línea base.
        var createContext = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var created = (InspectionContext)(object)createContext;
        var unlinkedProduct = Product.Create("Model two", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
        created.Products.Add(unlinkedProduct);
        created.Stores.Add(irion);
        var creation = new ConfirmIrionOfferHandler(createContext, new ApiResponseService(),
            new Extractor(available: true, price: 1200, original: 1500),
            new InspectIrionOfferHandler(createContext, new ApiResponseService()));
        var createdOffer = await creation.Handle(new(unlinkedProduct.ProductId, irion.StoreId, Source, "123", null, null,
            new IrionOfferSnapshot("CLP", 1200, 1500, true)), default);
        check(createdOffer.Success && createdOffer.Data?.Outcome == "Saved" && created.Rows.Count == 1 &&
            created.Histories.Count == 1 && created.Histories[0].IsBaseline &&
            created.Histories[0].InventoryId == created.Rows[0].InventoryId && created.Histories[0].Price == 1200 &&
            created.Histories[0].OriginalPrice == 1500,
            "(a) Creating an offer through the import path seeds exactly one baseline point");

        // (d) Modelo de EF.
        using var modelContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true").Options);
        var model = modelContext.GetService<IDesignTimeModel>().Model;
        var entity = model.FindEntityType(typeof(InventoryPriceHistory));
        check(entity is not null && entity.GetTableName() == "InventoryPriceHistories" &&
            entity.FindPrimaryKey()?.Properties.Select(p => p.Name).SequenceEqual(new[] { "PriceHistoryId" }) == true,
            "(d) The model exposes InventoryPriceHistory with its own key");
        if (entity is not null)
        {
            check(new[] { "InventoryId", "Price", "OriginalPrice", "DiscountPercentage", "ObservedAtUtc", "IsBaseline" }
                    .All(n => entity.FindProperty(n)?.IsNullable == false) &&
                entity.FindProperty("Availability")?.IsNullable == true &&
                entity.FindProperty("PriceHistoryId")?.GetDefaultValueSql() == "(newid())" &&
                entity.FindProperty("ObservedAtUtc")?.GetColumnType() == "datetime2",
                "(d) Required columns, nullable Availability and newid() key default");
            var index = entity.GetIndexes().SingleOrDefault(i =>
                i.Properties.Select(p => p.Name).SequenceEqual(new[] { "InventoryId", "ObservedAtUtc" }));
            check(index is { IsUnique: false } && index.GetFilter() is null &&
                index.GetDatabaseName() == "IX_InventoryPriceHistories_InventoryId_ObservedAtUtc",
                "(d) The series index exists on InventoryId, ObservedAtUtc");
            var foreignKey = entity.GetForeignKeys().SingleOrDefault();
            check(foreignKey is not null && foreignKey.PrincipalEntityType.ClrType == typeof(Inventory) &&
                foreignKey.DeleteBehavior == DeleteBehavior.Cascade &&
                foreignKey.Properties.Select(p => p.Name).SequenceEqual(new[] { "InventoryId" }),
                "(d) The offer relationship cascades on delete from Inventories");
        }

        // (e) Estructura de la migración, SQL de siembra y rollback.
        var migrations = modelContext.GetService<IMigrationsAssembly>();
        // Ya existe una migración posterior (condición y medio de pago), así que se busca por identificador.
        var newest = migrations.Migrations.OrderBy(m => m.Key, StringComparer.Ordinal)
            .LastOrDefault(m => m.Key.EndsWith("_InventoryPriceHistory"));
        check(newest.Value is not null, "(e) The price history migration is discoverable by its identifier");
        if (newest.Value is not null)
        {
            var migration = migrations.CreateMigration(newest.Value, modelContext.Database.ProviderName!);
            var up = migration.UpOperations;
            var table = up.OfType<CreateTableOperation>().SingleOrDefault();
            check(table is not null && table.Name == "InventoryPriceHistories" &&
                table.Columns.Select(c => c.Name).SequenceEqual(new[]
                    { "PriceHistoryId", "InventoryId", "Price", "OriginalPrice", "DiscountPercentage", "Availability", "ObservedAtUtc", "IsBaseline" }) &&
                table.PrimaryKey?.Name == "PK_InventoryPriceHistories" &&
                table.ForeignKeys.Single().PrincipalTable == "Inventories" &&
                table.ForeignKeys.Single().OnDelete == ReferentialAction.Cascade,
                "(e) Up creates the table with its key and cascading offer relationship");
            check(up.OfType<CreateIndexOperation>().Any(i => i.Table == "InventoryPriceHistories" &&
                i.Name == "IX_InventoryPriceHistories_InventoryId_ObservedAtUtc" && !i.IsUnique &&
                i.Columns.SequenceEqual(new[] { "InventoryId", "ObservedAtUtc" })),
                "(e) Up creates the series index");
            var seed = up.OfType<SqlOperation>().SingleOrDefault(s => s.Sql.Contains("INSERT INTO [InventoryPriceHistories]"));
            check(seed is not null && seed.Sql.Contains("[IsActive] = 1") && seed.Sql.Contains("NOT EXISTS") &&
                seed.Sql.Contains("SYSUTCDATETIME()") && seed.Sql.Contains("[IsBaseline]"),
                "(e) Up seeds one idempotent baseline per active offer with the migration instant");
            check(migration.DownOperations.OfType<DropTableOperation>().Any(t => t.Name == "InventoryPriceHistories") &&
                migration.DownOperations.Count == 1,
                "(e) Down drops the table and with it the seeded baseline rows");
            var generator = modelContext.GetService<IMigrationsSqlGenerator>();
            var sql = string.Join("\n", generator.Generate(up, model).Select(c => c.CommandText));
            check(sql.Contains("CREATE TABLE [InventoryPriceHistories]") && sql.Contains("ON DELETE CASCADE") &&
                sql.Contains("CREATE INDEX [IX_InventoryPriceHistories_InventoryId_ObservedAtUtc]") &&
                sql.Contains("SYSUTCDATETIME()"), "(e) SQL Server DDL generates the table, index and seed without a connection");
            var differ = modelContext.GetService<IMigrationsModelDiffer>();
            var initializer = modelContext.GetService<IModelRuntimeInitializer>();
            var target = initializer.Initialize(migration.TargetModel, designTime: true);
            // Dos migraciones posteriores agregan columnas: condición y medio de pago en Inventories, y grado,
            // escala y número de parte en Products son tres columnas nuevas. El objetivo de esta migración queda
            // atrás exactamente por esas cinco columnas y por nada más.
            var differences = differ.GetDifferences(target.GetRelationalModel(), model.GetRelationalModel());
            check(differences.OfType<AddColumnOperation>()
                    .Select(c => c.Table + "." + c.Name).OrderBy(n => n, StringComparer.Ordinal)
                    .SequenceEqual(new[]
                    {
                        "Inventories.Condition", "Inventories.PaymentMethodLabel", "Products.Grade",
                        "Products.PartNumber", "Products.Scale"
                    }) &&
                differences.Count == 5,
                "(e) The price history migration target differs from today's model only by the later additive columns");
            var file = Directory.EnumerateFiles("src/Infrastructure/Data/Migrations", "*_InventoryPriceHistory.cs")
                .FirstOrDefault();
            var text = file is null ? "" : File.ReadAllText(file);
            check(text.Contains("migrationBuilder.Sql(") && text.Contains("INSERT INTO [InventoryPriceHistories]") &&
                text.Contains("DropTable") && text.Contains("[InventoryPriceHistories]"),
                "(e) The migration file itself declares the seed SQL and the table drop");
        }
    }

    private sealed class Extractor(bool available, decimal price, decimal? original) : IIrionProductExtractor
    {
        public bool Available { get; set; } = available;
        public Task<IrionProductPreviewDto> ExtractAsync(string url, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var preview = new IrionProductPreviewDto(Source, "Model", "Irion", "CLP", "", [], [], Available,
                [new("123", null, null, Available, price, original, null)]);
            return Task.FromResult(preview);
        }
    }
}
