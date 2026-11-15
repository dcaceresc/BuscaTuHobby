using System.Reflection;
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

if (args.SequenceEqual(new[] { "--prepare-metadata" }))
{
    MigrationMetadata.Prepare();
    return 0;
}

var count = 0;
var failures = new List<string>();
void Check(bool condition, string message)
{
    count++;
    if (!condition) failures.Add(message);
}
void Reject(Action action, string message)
{
    try { action(); Check(false, message); }
    catch (ArgumentException) { Check(true, message); }
    catch (InvalidOperationException) { Check(true, message); }
}
static string State(Inventory row) => System.Text.Json.JsonSerializer.Serialize(row);
var product = Guid.NewGuid();
var store = Guid.NewGuid();
var legacy = Inventory.Create(product, store, 100, 0, 0);
var metadata = new[] { "SourceUrl", "ExternalVariantId", "Availability", "ObservedAtUtc" };
Check(metadata.All(n => typeof(Inventory).GetProperty(n) is not null), "Nullable source metadata contract exists");
foreach (var name in metadata)
    Check(typeof(Inventory).GetProperty(name)?.GetValue(legacy) is null, "Legacy has no fabricated " + name);
legacy.ToggleActive();
legacy.Update(product, store, 120, 150, 20);
Check(!legacy.IsActive && legacy.Price == 120 && legacy.OriginalPrice == 150 && legacy.DiscountPercentage == 20,
    "Manual update preserves disabled state and valid prices");
await ManualNumericChecks.Run(Check);
foreach (var ids in new[] { (Guid.Empty, store), (product, Guid.Empty) })
    Reject(() => Inventory.Create(ids.Item1, ids.Item2, 100, 0, 0), "Reject empty references");
var createImported = typeof(Inventory).GetMethod("CreateImportedOffer");
var updateImported = typeof(Inventory).GetMethod("UpdateImportedOffer");
Check(createImported is not null && updateImported is not null, "Dedicated imported offer methods exist");
if (createImported is not null && updateImported is not null)
{
    const string source = "https://irion.cl/products/model";
    var observed = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc);
    Inventory Create(string url, string variant, DateTime time) =>
        (Inventory)Invoke(createImported, null, [product, store, 100, 200, 50, url, variant, true, time])!;
    void Update(Inventory row, string url, string variant, DateTime time, bool available = false) =>
        Invoke(updateImported, row, [90, 100, 10, url, variant, available, time]);
    var imported = Create(source, "123", observed);
    imported.ToggleActive();
    Update(imported, source, "123", observed.AddMinutes(1));
    Check(!imported.IsActive && imported.Price == 90 && (bool?)typeof(Inventory).GetProperty("Availability")!.GetValue(imported) == false,
        "Imported unavailable update never reactivates disabled row");
    imported.Update(product, store, 80, 100, 20);
    Check((string?)typeof(Inventory).GetProperty("ExternalVariantId")!.GetValue(imported) == "123" &&
        (DateTime?)typeof(Inventory).GetProperty("ObservedAtUtc")!.GetValue(imported) == observed.AddMinutes(1) && !imported.IsActive,
        "Manual same-pair edit preserves identity and disabled state");
    var unchanged = State(imported);
    foreach (var ids in new[] { (Guid.NewGuid(), store), (product, Guid.NewGuid()) })
    {
        Reject(() => imported.Update(ids.Item1, ids.Item2, 70, 0, 0), "Imported pair cannot change silently");
        Check(State(imported) == unchanged, "Association rejection is atomic");
    }
    foreach (var identity in new[] { (source, "124"), (source + "-other", "123") })
    {
        Reject(() => Update(imported, identity.Item1, identity.Item2, observed.AddMinutes(2)), "Identity cannot be overwritten");
        Check(State(imported) == unchanged, "Identity rejection does not mutate");
    }
    var legacyBefore = State(legacy);
    Reject(() => Update(legacy, source, "123", observed), "Legacy pair cannot be adopted");
    Check(State(legacy) == legacyBefore, "Rejected adoption preserves every legacy field");
    foreach (var prices in new[] { (0, 0, 0), (-1, 0, 0), (100, -1, 0), (100, 90, 0), (100, 0, 1),
        (100, 200, -1), (100, 200, 101), (int.MinValue, 1, 0), (100, 200, 49) })
    {
        Reject(() => Invoke(createImported, null, [product, store, prices.Item1, prices.Item2, prices.Item3, source, "123", true, observed]),
            "Imported factory rejects price metadata " + prices);
        Reject(() => Invoke(updateImported, imported, [prices.Item1, prices.Item2, prices.Item3, source, "123", true, observed]),
            "Imported price metadata rejected " + prices);
        Check(State(imported) == unchanged, "Imported validation is atomic " + prices);
    }
    var maximum = (Inventory)Invoke(createImported, null, [product, store, int.MaxValue, int.MaxValue, 0, source, "123", false, observed])!;
    Check(maximum.Price == int.MaxValue && maximum.IsActive, "Int32 maximum accepted; unavailable does not disable administrative state");
    var rounded = (Inventory)Invoke(createImported, null, [product, store, 199, 200, 1, source, "123", true, observed])!;
    Check(rounded.DiscountPercentage == 1, "Imported midpoint discount rounds away from zero");
    foreach (var input in new[] { ("", "123", observed), (source, "", observed), (source, "123", observed.ToLocalTime()), (source, "123", default(DateTime)) })
        Reject(() => Create(input.Item1, input.Item2, input.Item3), "Reject invalid import metadata");
}

await ManualChecks.Run(Check, product, store);
await InspectionChecks.Run(Check);
await ConfirmationChecks.Run(Check);
await DashboardChecks.Run(Check);
using var context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer("Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true").Options);
var model = context.GetService<IDesignTimeModel>().Model;
var entity = model.FindEntityType(typeof(Inventory))!;
foreach (var name in metadata)
    Check(entity.FindProperty(name)?.IsNullable == true, "Nullable model field " + name);
var token = entity.FindProperty("RowVersion");
Check(token is { IsConcurrencyToken: true, ValueGenerated: ValueGenerated.OnAddOrUpdate } && token.GetColumnType() == "rowversion",
    "Database-generated rowversion concurrency token");
Check(entity.GetIndexes().Any(i => i.IsUnique && i.GetFilter() is null &&
    i.Properties.Select(p => p.Name).SequenceEqual(new[] { "StoreId", "ProductId" })), "Unique pair includes disabled and legacy rows");
var migrations = context.GetService<IMigrationsAssembly>();
var migrationType = migrations.Migrations.SingleOrDefault(m => m.Key.EndsWith("_IrionOffer"));
Check(migrationType.Value is not null, "Conventional discoverable migration exists");
if (migrationType.Value is not null)
{
    var migration = migrations.CreateMigration(migrationType.Value, context.Database.ProviderName!);
    var operations = migration.UpOperations;
    Check(operations.FirstOrDefault() is SqlOperation sql && sql.Sql.Contains("HAVING COUNT(*) > 1") && sql.Sql.Contains("THROW"),
        "Duplicate guard precedes all schema changes");
    Check(!operations.OfType<SqlOperation>().Any(s => s.Sql.Contains("DELETE", StringComparison.OrdinalIgnoreCase) ||
        s.Sql.Contains("UPDATE", StringComparison.OrdinalIgnoreCase)), "No historical backfill or cleanup");
    Check(operations.OfType<AddColumnOperation>().Count(c => c.IsNullable) == 4, "Four nullable additive metadata columns");
    Check(migration.DownOperations.OfType<DropColumnOperation>().Count() == 5 &&
        migration.DownOperations.OfType<CreateIndexOperation>().Any(i => i.Name == "IX_Inventories_StoreId"), "Coherent rollback restores old index");
    var generator = context.GetService<IMigrationsSqlGenerator>();
    var up = string.Join("\n", generator.Generate(operations, model).Select(c => c.CommandText));
    Check(up.IndexOf("THROW", StringComparison.Ordinal) < up.IndexOf("ALTER TABLE", StringComparison.Ordinal) &&
        up.Contains("CREATE UNIQUE INDEX [IX_Inventories_StoreId_ProductId]") && up.Contains("rowversion"), "SQL Server DDL contains guard, unique index and token");
    Check(generator.Generate(migration.DownOperations, model).Count > 0, "Rollback SQL generates without connection");
    var differ = context.GetService<IMigrationsModelDiffer>();
    var initializer = context.GetService<IModelRuntimeInitializer>();
    var target = initializer.Initialize(migration.TargetModel, designTime: true);
    var snapshot = initializer.Initialize(migrations.ModelSnapshot!.Model, designTime: true);
    Check(!differ.HasDifferences(target.GetRelationalModel(), model.GetRelationalModel()), "Migration target matches current model");
    Check(!differ.HasDifferences(snapshot.GetRelationalModel(), model.GetRelationalModel()), "Snapshot matches current model");
}
if (failures.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
    Console.Error.WriteLine($"FAIL: {failures.Count} of {count} assertions.");
    return 1;
}
Console.WriteLine($"PASS: {count} assertions; structural SQL/model and fake orchestration only; no database or HTTP used.");
return 0;

static object? Invoke(MethodInfo method, object? target, object[] args)
{
    try { return method.Invoke(target, args); }
    catch (TargetInvocationException e) when (e.InnerException is not null)
    { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw(); throw; }
}
