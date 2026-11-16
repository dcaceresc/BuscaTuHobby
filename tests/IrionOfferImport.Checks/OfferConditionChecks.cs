using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Application.Maintainer.Inventories.Queries.GetInventories;
using Application.Maintainer.Inventories.Queries.GetInventoryById;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

// Condición de la oferta y medio de pago: contrato entero 0/1/2, validación en la entidad y en los handlers,
// modelo de EF y forma de la migración escrita a mano. Todo contra dobles en memoria y generación de SQL sin
// conexión: aquí no se aplica ninguna migración ni se usa una base de datos.
internal static class OfferConditionChecks
{
    private const string InvalidCondition = OfferConditionRules.InvalidConditionMessage;
    private const string InvalidLabel = OfferConditionRules.InvalidPaymentMethodLabelMessage;

    public static async Task Run(Action<bool, string> check)
    {
        EnumContract(check);
        await ManualWriteSide(check);
        await RejectionPersistsNothing(check);
        ModelShape(check);
        MigrationShape(check);
        ReadSideShape(check);
    }

    private static void EnumContract(Action<bool, string> check)
    {
        var values = Enum.GetValues<ProductCondition>();
        check(values.SequenceEqual(new[] { ProductCondition.New, ProductCondition.Refurbished, ProductCondition.Used }) &&
            (int)ProductCondition.New == 0 && (int)ProductCondition.Refurbished == 1 && (int)ProductCondition.Used == 2 &&
            Enum.GetUnderlyingType(typeof(ProductCondition)) == typeof(int) && values.Length == 3,
            "The condition contract is exactly three integer values 0 new, 1 refurbished, 2 used");

        // El mensaje de rechazo usa el mismo vocabulario que la interfaz: "reutilizada", no "refabricada".
        check(InvalidCondition == "La condición de la oferta debe ser 0 (nueva), 1 (reutilizada) o 2 (usada).",
            "The invalid condition message keeps its wording and names the value 1 reutilizada like the interface");

        var product = Guid.NewGuid();
        var store = Guid.NewGuid();
        var offer = Inventory.Create(product, store, 100, 0, 0);
        check(offer.Condition == ProductCondition.New && offer.PaymentMethodLabel is null,
            "An offer created without details defaults to new and no payment method");

        var imported = Inventory.CreateImportedOffer(product, store, 100, 0, 0, "https://irion.cl/products/model",
            "123", true, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        check(imported.Condition == ProductCondition.New && imported.PaymentMethodLabel is null,
            "An imported offer without condition metadata stays new and has no payment method");

        var detailed = Inventory.Create(product, store, 100, 0, 0, ProductCondition.Used, "Tarjeta Ripley");
        check(detailed.Condition == ProductCondition.Used && detailed.PaymentMethodLabel == "Tarjeta Ripley",
            "The factory stores the condition and the payment method it receives");
        check(Inventory.Create(product, store, 100, 0, 0, ProductCondition.Refurbished, new string('a', 64))
                .PaymentMethodLabel == new string('a', 64),
            "A payment method of exactly 64 characters is accepted");
    }

    private static async Task ManualWriteSide(Action<bool, string> check)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
        var fake = (ContextProxy)(object)context;
        var services = new ApiResponseService();
        var product = Guid.NewGuid();
        var store = Guid.NewGuid();

        var response = await new CreateInventoryHandler(context, services)
            .Handle(new(product, store, 100, 0, 0, (int)ProductCondition.Refurbished, "Tarjeta CMR"), default);
        var row = fake.Rows.SingleOrDefault();
        check(response.Success && response.Message == "Inventario creado correctamente" && row is not null &&
            row.Condition == ProductCondition.Refurbished && row.PaymentMethodLabel == "Tarjeta CMR",
            "Creating an offer keeps the condition and payment method it was created with");
        check(fake.Saves == 1, "Creating an offer with details still saves exactly once");

        var offer = fake.Rows.Single();
        var stored = row!;
        response = await new UpdateInventoryHandler(context, services).Handle(new()
        {
            InventoryId = offer.InventoryId, ProductId = product, StoreId = store,
            Price = 120, OriginalPrice = 150, DiscountPercentage = 20,
            Condition = (int)ProductCondition.Used, PaymentMethodLabel = "Tarjeta Ripley"
        }, default);
        check(response.Success && response.Message == "Inventario actualizado correctamente" &&
            stored.Condition == ProductCondition.Used && stored.PaymentMethodLabel == "Tarjeta Ripley",
            "Updating an offer changes both the condition and the payment method");

        response = await new UpdateInventoryHandler(context, services).Handle(new()
        {
            InventoryId = offer.InventoryId, ProductId = product, StoreId = store,
            Price = 120, OriginalPrice = 150, DiscountPercentage = 20,
            Condition = (int)ProductCondition.New, PaymentMethodLabel = null
        }, default);
        check(response.Success && stored.Condition == ProductCondition.New && stored.PaymentMethodLabel is null,
            "An update can return the offer to new and clear the payment method");

        // La importación solo refresca precios: la condición y el medio de pago ya registrados sobreviven.
        var imported = Inventory.CreateImportedOffer(product, Guid.NewGuid(), 100, 0, 0,
            "https://irion.cl/products/model", "123", true, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
            ProductCondition.Used, "Tarjeta Ripley");
        imported.UpdateImportedOffer(90, 0, 0, "https://irion.cl/products/model", "123", true,
            new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), imported.Condition, imported.PaymentMethodLabel);
        check(imported.Price == 90 && imported.Condition == ProductCondition.Used &&
            imported.PaymentMethodLabel == "Tarjeta Ripley",
            "An imported price refresh keeps the condition and payment method already registered");
    }

    private static async Task RejectionPersistsNothing(Action<bool, string> check)
    {
        var services = new ApiResponseService();
        var product = Guid.NewGuid();
        var store = Guid.NewGuid();

        foreach (var invalid in new[] { 3, -1, int.MaxValue, int.MinValue })
        {
            var context = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
            var fake = (ContextProxy)(object)context;
            var response = await new CreateInventoryHandler(context, services)
                .Handle(new(product, store, 100, 0, 0, invalid, "Tarjeta CMR"), default);
            check(!response.Success && response.Message == InvalidCondition &&
                fake.Rows.Count == 0 && fake.Histories.Count == 0 && fake.Saves == 0,
                "Out-of-range condition " + invalid + " is rejected with the exact Spanish message and persists nothing");
        }

        var updateContext = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
        var updateFake = (ContextProxy)(object)updateContext;
        var created = await new CreateInventoryHandler(updateContext, services)
            .Handle(new(product, store, 100, 0, 0, (int)ProductCondition.Refurbished, "Tarjeta CMR"), default);
        var row = updateFake.Rows.Single();
        var stored = JsonSerializer.Serialize(updateFake.Rows);
        var histories = updateFake.Histories.Count;
        var saves = updateFake.Saves;
        var update = await new UpdateInventoryHandler(updateContext, services).Handle(new()
        {
            InventoryId = row.InventoryId, ProductId = product, StoreId = store,
            Price = 120, OriginalPrice = 0, DiscountPercentage = 0,
            Condition = 3, PaymentMethodLabel = "Tarjeta Ripley"
        }, default);
        check(created.Success && !update.Success && update.Message == InvalidCondition &&
            JsonSerializer.Serialize(updateFake.Rows) == stored && updateFake.Histories.Count == histories &&
            updateFake.Saves == saves,
            "An update with an out-of-range condition is rejected with the exact message and changes nothing");

        foreach (var label in new[] { new string('a', 65), new string('a', 200), "", "   ", " Tarjeta CMR" })
        {
            var labelContext = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
            var labelFake = (ContextProxy)(object)labelContext;
            var response = await new CreateInventoryHandler(labelContext, services)
                .Handle(new(product, store, 100, 0, 0, (int)ProductCondition.New, label), default);
            check(!response.Success && response.Message == InvalidLabel &&
                labelFake.Rows.Count == 0 && labelFake.Saves == 0,
                "An invalid payment method label of " + label.Length + " characters is rejected and persists nothing");
        }

        var entityRejections = 0;
        foreach (var action in new Action[]
                 {
                     () => Inventory.Create(product, store, 100, 0, 0, (ProductCondition)3),
                     () => Inventory.Create(product, store, 100, 0, 0, ProductCondition.New, new string('a', 65)),
                     () => Inventory.Create(product, store, 100, 0, 0, ProductCondition.New, "")
                 })
        {
            try { action(); }
            catch (ArgumentException) { entityRejections++; }
        }
        check(entityRejections == 3, "The entity itself rejects an undefined condition and an invalid payment label");
    }

    private static void ModelShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var entity = model.FindEntityType(typeof(Inventory));
        var condition = entity?.FindProperty(nameof(Inventory.Condition));
        var label = entity?.FindProperty(nameof(Inventory.PaymentMethodLabel));
        check(condition is not null && condition.ClrType == typeof(ProductCondition) && !condition.IsNullable &&
            condition.GetColumnType() == "int" && Convert.ToInt32(condition.GetDefaultValue()) == 0,
            "The model maps the condition as a required int column with a database default of 0");
        check(label is not null && label.IsNullable && label.GetMaxLength() == 64 &&
            label.GetColumnType() == "nvarchar(64)",
            "The model maps the payment method label as a nullable nvarchar(64) column");
    }

    private static void MigrationShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var migrations = context.GetService<IMigrationsAssembly>();
        // Una migración posterior (grado, escala y número de parte) pasa a ser la más nueva: la de condición y
        // medio de pago se busca por identificador en lugar de asumir que sigue siendo la última.
        var newest = migrations.Migrations.OrderBy(m => m.Key, StringComparer.Ordinal)
            .LastOrDefault(m => m.Key.EndsWith("_OfferConditionAndPaymentMethod"));
        check(newest.Key == "20260722000000_OfferConditionAndPaymentMethod" &&
            string.CompareOrdinal(newest.Key, "20260721000000_InventoryPriceHistory") > 0,
            "The offer condition migration is discoverable and sorts after the price history migration");
        var migration = migrations.CreateMigration(newest.Value!, context.Database.ProviderName!);
        var up = migration.UpOperations;
        var condition = up.OfType<AddColumnOperation>().SingleOrDefault(c => c.Name == "Condition");
        var label = up.OfType<AddColumnOperation>().SingleOrDefault(c => c.Name == "PaymentMethodLabel");
        check(up.Count == 2 && condition is not null && condition.Table == "Inventories" &&
            condition.ColumnType == "int" && !condition.IsNullable && condition.DefaultValue is 0,
            "Up adds the condition as int NOT NULL DEFAULT 0 so existing rows become new");
        check(label is not null && label.Table == "Inventories" && label.ColumnType == "nvarchar(64)" &&
            label.IsNullable && label.MaxLength == 64,
            "Up adds the payment method label as nullable nvarchar(64)");
        var down = migration.DownOperations;
        check(down.Count == 2 &&
            down.OfType<DropColumnOperation>().Select(c => c.Table + "." + c.Name).OrderBy(n => n, StringComparer.Ordinal)
                .SequenceEqual(new[] { "Inventories.Condition", "Inventories.PaymentMethodLabel" }),
            "Down drops both columns and nothing else");

        var generator = context.GetService<IMigrationsSqlGenerator>();
        var sql = string.Join("\n", generator.Generate(up, model).Select(c => c.CommandText));
        check(sql.Contains("ALTER TABLE [Inventories] ADD [Condition] int NOT NULL DEFAULT 0") &&
            sql.Contains("ALTER TABLE [Inventories] ADD [PaymentMethodLabel] nvarchar(64) NULL"),
            "SQL Server DDL adds both columns with the default and the nullability");
        var downSql = string.Join("\n",
            generator.Generate(down, model).Select(c => c.CommandText));
        check(downSql.Contains("DROP COLUMN [Condition]") && downSql.Contains("DROP COLUMN [PaymentMethodLabel]"),
            "SQL Server rollback drops both columns");

        var file = Directory.EnumerateFiles("src/Infrastructure/Data/Migrations", "*_OfferConditionAndPaymentMethod.cs")
            .SingleOrDefault();
        var text = file is null ? "" : File.ReadAllText(file);
        check(text.Contains("AddColumn<int>") && text.Contains("defaultValue: 0") &&
            text.Contains("maxLength: 64") && text.Contains("DropColumn"),
            "The hand-written migration file itself declares the default, the length and the rollback");
    }

    private static void ReadSideShape(Action<bool, string> check)
    {
        check(typeof(InventoryVM).GetProperty(nameof(InventoryVM.Condition))?.PropertyType == typeof(int) &&
            typeof(InventoryVM).GetProperty(nameof(InventoryVM.PaymentMethodLabel))?.PropertyType == typeof(string),
            "The maintainer detail view model exposes the integer condition and the payment method label");
        check(typeof(InventoryDto).GetProperty(nameof(InventoryDto.Condition))?.PropertyType == typeof(int) &&
            typeof(InventoryDto).GetProperty(nameof(InventoryDto.PaymentMethodLabel))?.PropertyType == typeof(string),
            "The maintainer list row exposes the integer condition and the payment method label");
        check(new[] { "GetInventories.cs", "GetInventoryById.cs" }.All(name =>
                Directory.EnumerateFiles("src/Application/Maintainer/Inventories/Queries", name, SearchOption.AllDirectories)
                    .Select(File.ReadAllText).SingleOrDefault() is { } source &&
                source.Contains("Condition = (int)x.Condition") && source.Contains("PaymentMethodLabel = x.PaymentMethodLabel")),
            "Both maintainer queries project the condition and the payment method label from the offer");
    }

    private static ApplicationDbContext StructuralContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true")
            .Options);
}
