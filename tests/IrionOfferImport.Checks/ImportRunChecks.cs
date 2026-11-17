using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

// Ingesta por sitemap: entidades de la ejecución, su modelo y su migración, el presupuesto de peticiones y la
// regla de idempotencia y reanudación. Sin base de datos y sin red: la estructura es el modelo de EF y la regla
// se ejerce sobre entidades en memoria.
internal static class ImportRunChecks
{
    private const string RunStore = "La tienda de la ejecución es obligatoria.";
    private const string RunBudget = "El presupuesto de peticiones de la ejecución debe ser positivo.";
    private const string RunStart = "El inicio de la ejecución debe ser una fecha UTC.";
    private const string RunFinish = "El fin de la ejecución debe ser una fecha UTC.";
    private const string RunFinishBeforeStart = "El fin de la ejecución no puede ser anterior al inicio.";
    private const string RunClosed = "La ejecución ya está cerrada.";
    private const string RunCompleted = "Una ejecución completada no se reanuda: crea una ejecución nueva.";
    private const string RunCount = "El contador de la ejecución no puede ser negativo.";
    private const string ItemRun = "La ejecución del elemento es obligatoria.";
    private const string ItemUrl = "La URL de origen del elemento no es válida.";
    private const string ItemError = "El mensaje de error del elemento no es válido.";
    private const string ItemStatus = "El código HTTP del elemento no es válido.";
    private const string ItemAttempt = "La fecha del intento debe ser una fecha UTC.";
    private const string ItemForeignRun = "Los elementos no pertenecen a la ejecución.";
    private const string BudgetMessage = "El presupuesto de peticiones debe ser positivo.";

    public static void Run(Action<bool, string> check)
    {
        EntityContract(check);
        ItemContract(check);
        BudgetContract(check);
        ModelShape(check);
        MigrationShape(check);
        PlannerContract(check);
    }

    private static void EntityContract(Action<bool, string> check)
    {
        var storeId = Guid.NewGuid();
        var start = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);
        var run = ImportRun.Create(storeId, 500, start);
        check(run.StoreId == storeId && run.RequestBudget == 500 && run.StartedAtUtc == start,
            "A new run keeps the store, the budget it started with and the start time");
        check(run.Status == ImportRunStatus.Running && run.FinishedAtUtc is null &&
            run.DiscoveredCount == 0 && run.StoredCount == 0 && run.SkippedCount == 0 && run.FailedCount == 0,
            "A new run is open and its counters start at zero");

        run.RecordDiscovered(3);
        run.RecordStored(2);
        run.RecordSkipped(1);
        run.RecordFailed(4);
        check(run.DiscoveredCount == 3 && run.StoredCount == 2 && run.SkippedCount == 1 && run.FailedCount == 4,
            "Each counter records what actually happened to the run");
        run.RecordDiscovered(2);
        check(run.DiscoveredCount == 5, "Counters accumulate batch by batch");

        var finished = start.AddMinutes(1);
        run.Complete(finished);
        check(run.Status == ImportRunStatus.Completed && run.FinishedAtUtc == finished,
            "Closing the run writes its finish time and its final status");
        Reject(() => run.RecordStored(1), RunClosed);
        check(run.StoredCount == 2 && run.Status == ImportRunStatus.Completed,
            "A closed run cannot be changed and keeps every counter");
        Reject(() => run.Complete(start.AddMinutes(2)), RunClosed);
        Reject(() => run.Resume(), RunCompleted);

        var failing = ImportRun.Create(storeId, 10, start);
        failing.RecordFailed(1);
        failing.Fail(start.AddSeconds(5));
        check(failing.Status == ImportRunStatus.Failed && failing.FinishedAtUtc == start.AddSeconds(5),
            "An interrupted run is recorded as failed with its finish time");
        failing.Resume();
        check(failing.Status == ImportRunStatus.Running && failing.FinishedAtUtc is null && failing.FailedCount == 1,
            "Resuming reopens an unfinished run and keeps what it already counted");
        failing.Complete(start.AddSeconds(9));

        foreach (var budget in new[] { 0, -1 })
            Reject(() => ImportRun.Create(storeId, budget, start), RunBudget);
        Reject(() => ImportRun.Create(Guid.Empty, 500, start), RunStore);
        foreach (var invalidStart in new[]
                 {
                     default(DateTime), start.ToLocalTime(),
                     DateTime.SpecifyKind(start, DateTimeKind.Unspecified)
                 })
            Reject(() => ImportRun.Create(storeId, 500, invalidStart), RunStart);

        var guarded = ImportRun.Create(storeId, 500, start);
        foreach (var invalidFinish in new[]
                 {
                     default(DateTime), DateTime.SpecifyKind(start.AddMinutes(1), DateTimeKind.Unspecified),
                     start.AddMinutes(-1)
                 })
        {
            Reject(() => guarded.Complete(invalidFinish),
                invalidFinish == start.AddMinutes(-1) ? RunFinishBeforeStart : RunFinish);
            check(guarded.Status == ImportRunStatus.Running && guarded.FinishedAtUtc is null,
                "A rejected finish leaves the run open: " + invalidFinish.Kind);
        }

        Reject(() => guarded.RecordSkipped(-1), RunCount);
        check(guarded.SkippedCount == 0, "A rejected count never mutates the run");
    }

    private static void ItemContract(Action<bool, string> check)
    {
        var runId = Guid.NewGuid();
        var at = new DateTime(2026, 11, 16, 12, 30, 0, DateTimeKind.Utc);
        const string url = "https://irion.cl/products/optimus-prime";

        var pending = ImportItem.Create(runId, url);
        check(pending.ImportRunId == runId && pending.SourceUrl == url && pending.State == ImportItemState.Pending &&
            pending.HttpStatus is null && pending.ErrorMessage is null && pending.AttemptedAtUtc is null,
            "A discovered URL starts pending, with nothing attempted yet");
        check(pending.IsProcessable && !pending.IsTransientFailure, "A pending item is work to do");

        var skipped = ImportItem.CreateSkipped(runId, url, CatalogSkip.RobotsDisallowedMessage);
        check(skipped.State == ImportItemState.Skipped && skipped.HttpStatus is null &&
            skipped.AttemptedAtUtc is null && skipped.ErrorMessage == "robots.txt prohíbe esta ruta: no se consultó." &&
            !skipped.IsProcessable,
            "A skip without a request records the reason and no HTTP status");
        var alreadyStored = ImportItem.CreateSkipped(runId, url, CatalogSkip.AlreadyStoredMessage);
        check(alreadyStored.ErrorMessage == "Una ejecución completada anterior ya guardó esta URL." &&
            !alreadyStored.IsProcessable,
            "An idempotent skip says why the URL was not queued again");

        var stored = ImportItem.Create(runId, url);
        stored.MarkStored(200, at);
        check(stored.State == ImportItemState.Stored && stored.HttpStatus == 200 && stored.AttemptedAtUtc == at &&
            stored.ErrorMessage is null && !stored.IsProcessable,
            "Storing an item records the status and the attempt time");

        foreach (var status in new int?[] { null, 429, 500, 503 })
        {
            var item = ImportItem.Create(runId, url);
            item.MarkFailed(status, "No se pudo leer la ficha.", at);
            check(item.State == ImportItemState.Failed && item.IsTransientFailure && item.IsProcessable &&
                item.ErrorMessage == "No se pudo leer la ficha.",
                "A transient failure is retried on resume: " + (status?.ToString() ?? "sin respuesta"));
        }

        foreach (var status in new int?[] { 200, 400, 404, 410 })
        {
            var item = ImportItem.Create(runId, url);
            item.MarkFailed(status, "La ficha ya no existe.", at);
            check(!item.IsTransientFailure && !item.IsProcessable,
                "A definitive failure is not retried on resume: " + status);
        }

        var discarded = ImportItem.Create(runId, url);
        discarded.MarkSkipped(404, "La ficha ya no existe.", at);
        check(discarded.State == ImportItemState.Skipped && discarded.HttpStatus == 404 &&
            discarded.AttemptedAtUtc == at && !discarded.IsProcessable,
            "A requested but rejected entry is recorded as skipped with its status");

        var retried = ImportItem.Create(runId, url);
        retried.MarkFailed(503, "Servicio no disponible.", at);
        retried.MarkStored(200, at.AddMinutes(1));
        check(retried.State == ImportItemState.Stored && retried.HttpStatus == 200 &&
            retried.ErrorMessage is null && retried.AttemptedAtUtc == at.AddMinutes(1),
            "A retry overwrites the failure it recovered from");

        foreach (var invalid in new[] { null, "", "   ", "http://irion.cl/products/a", "not a url",
                     "https://user@irion.cl/products/a", url + " ", new string('u', 2049) })
            Reject(() => ImportItem.Create(runId, invalid!), ItemUrl);
        foreach (var invalid in new[] { null, "", "   ", " error", new string('e', 513) })
            Reject(() => ImportItem.CreateSkipped(runId, url, invalid!), ItemError);
        Reject(() => ImportItem.Create(Guid.Empty, url), ItemRun);
        var guard = ImportItem.Create(runId, url);
        foreach (var status in new[] { 99, 600 })
        {
            Reject(() => guard.MarkStored(status, at), ItemStatus);
            Reject(() => guard.MarkFailed(status, "fallo", at), ItemStatus);
        }

        foreach (var attempt in new[] { default(DateTime), at.ToLocalTime() })
        {
            Reject(() => guard.MarkStored(200, attempt), ItemAttempt);
            check(guard.State == ImportItemState.Pending && guard.HttpStatus is null && guard.AttemptedAtUtc is null,
                "A rejected attempt leaves the item pending and untouched");
        }

        check(ImportItem.IsValidSourceUrl("https://luffytoys.cl/products/gundam-rx-78") &&
            !ImportItem.IsValidSourceUrl("https://irion.cl/products/a?x=1 "),
            "The audited URL is exactly what the store published");
    }

    private static void BudgetContract(Action<bool, string> check)
    {
        var budget = new RequestBudget(3);
        check(budget.Total == 3 && budget.Remaining == 3 && budget.Consumed == 0,
            "A run starts with the whole budget available");
        check(budget.TryConsume() && budget.Remaining == 2 && budget.Consumed == 1,
            "One request consumes one unit of the budget");
        budget.TryConsume();
        budget.TryConsume();
        check(!budget.TryConsume() && budget.Remaining == 0 && budget.Consumed == 3,
            "An exhausted budget refuses more requests instead of going negative");
        foreach (var total in new[] { 0, -1 })
            RejectMessage(() => _ = new RequestBudget(total), BudgetMessage);
    }

    private static void ModelShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var runs = model.FindEntityType(typeof(ImportRun));
        var items = model.FindEntityType(typeof(ImportItem));
        check(runs is not null && items is not null, "The model maps both ingestion entities");
        if (runs is null || items is null) return;

        check(items.GetIndexes().Any(index => index.IsUnique &&
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[] { "ImportRunId", "SourceUrl" })),
            "A run item is unique per run and source URL");
        check(items.GetIndexes().Any(index => !index.IsUnique &&
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[] { "ImportRunId", "State" })),
            "Resuming looks items up by run and state");
        check(items.FindProperty("SourceUrl") is { IsNullable: false } sourceUrl &&
            sourceUrl.GetMaxLength() == 2048 && sourceUrl.GetColumnType() == "nvarchar(2048)",
            "The source URL is a required nvarchar(2048)");
        check(items.FindProperty("ErrorMessage") is { IsNullable: true } error &&
            error.GetMaxLength() == 512 && error.GetColumnType() == "nvarchar(512)",
            "The error message is optional and bounded");
        check(items.FindProperty("State") is { IsNullable: false } state &&
            state.GetColumnType() == "int",
            "The item state is stored as the enum number");
        check(items.FindProperty("HttpStatus")?.IsNullable == true &&
            items.FindProperty("AttemptedAtUtc")?.IsNullable == true,
            "A skip without a request leaves the status and the attempt time null");
        var itemRun = items.GetForeignKeys().SingleOrDefault();
        check(itemRun is not null && itemRun.DeleteBehavior == DeleteBehavior.Cascade &&
            itemRun.PrincipalEntityType.ClrType == typeof(ImportRun),
            "Deleting a run deletes only its own items");

        check(runs.FindProperty("Status") is { IsNullable: false } status &&
            status.GetColumnType() == "int" && runs.FindProperty("StartedAtUtc")?.IsNullable == false &&
            runs.FindProperty("FinishedAtUtc")?.IsNullable == true,
            "The run stores its status as the enum number and a finish time only when it closes");
        check(runs.FindProperty("RequestBudget")?.IsNullable == false,
            "The run keeps the request budget it started with");
        var runStore = runs.GetForeignKeys().SingleOrDefault();
        check(runStore is not null && runStore.DeleteBehavior == DeleteBehavior.Restrict &&
            runStore.PrincipalEntityType.ClrType == typeof(Store),
            "Deleting a store never deletes the audit of its runs");
        check(runs.GetIndexes().Any(index =>
                index.Properties.Select(property => property.Name).SequenceEqual(new[] { "StoreId" })),
            "Idempotency queries runs by store");
    }

    private static void MigrationShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var migrations = context.GetService<IMigrationsAssembly>();
        var entry = migrations.Migrations.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .LastOrDefault(pair => pair.Key.EndsWith("_ImportRuns", StringComparison.Ordinal));
        check(entry.Value is not null && entry.Key == "20261116000001_ImportRuns" &&
            string.CompareOrdinal(entry.Key, "20261116000000_StoreConnectorConfiguration") > 0,
            "The ImportRuns migration is discoverable and sorts after the connector configuration");
        if (entry.Value is null) return;

        var migration = migrations.CreateMigration(entry.Value, context.Database.ProviderName!);
        var up = migration.UpOperations;
        var created = up.OfType<CreateTableOperation>().OrderBy(table => table.Name, StringComparer.Ordinal).ToArray();
        check(created.Length == 2 && created[0].Name == "ImportItems" && created[1].Name == "ImportRuns",
            "Up creates exactly the two ingestion tables");
        if (created.Length != 2) return;

        check(created[1].Columns.Select(column => column.Name).OrderBy(name => name, StringComparer.Ordinal)
                .SequenceEqual(new[]
                {
                    "DiscoveredCount", "FailedCount", "FinishedAtUtc", "ImportRunId", "RequestBudget",
                    "SkippedCount", "StartedAtUtc", "Status", "StoredCount", "StoreId"
                }.OrderBy(name => name, StringComparer.Ordinal)),
            "ImportRuns stores the store, the budget, the window and the four counters");
        check(created[0].Columns.Select(column => column.Name).OrderBy(name => name, StringComparer.Ordinal)
                .SequenceEqual(new[]
                {
                    "AttemptedAtUtc", "ErrorMessage", "HttpStatus", "ImportItemId", "ImportRunId", "SourceUrl", "State"
                }.OrderBy(name => name, StringComparer.Ordinal)),
            "ImportItems stores the run, the source URL, the state, the status, the error and the attempt");

        // La clave foránea declarada dentro de CreateTable viaja en la tabla, no como operación suelta.
        var foreignKeys = created.SelectMany(table => table.ForeignKeys).ToArray();
        check(foreignKeys.Length == 2 &&
            foreignKeys.Single(key => key.Name == "FK_ImportRuns_Stores_StoreId").OnDelete == ReferentialAction.Restrict &&
            foreignKeys.Single(key => key.Name == "FK_ImportItems_ImportRuns_ImportRunId").OnDelete == ReferentialAction.Cascade,
            "Up wires the store restriction and the run cascade exactly once each");

        var indexes = up.OfType<CreateIndexOperation>().ToArray();
        check(indexes.Any(index => index.Name == "IX_ImportItems_ImportRunId_SourceUrl" && index.IsUnique &&
                index.Columns.SequenceEqual(new[] { "ImportRunId", "SourceUrl" })),
            "Up declares the unique run/URL index as the duplicate guard");
        check(indexes.Any(index => index.Name == "IX_ImportItems_ImportRunId_State" && !index.IsUnique &&
                index.Columns.SequenceEqual(new[] { "ImportRunId", "State" })),
            "Up declares the resume index by run and state");
        check(indexes.Any(index => index.Name == "IX_ImportRuns_StoreId"),
            "Up indexes the runs of a store");

        var down = migration.DownOperations;
        check(down.Count == 2 && down.OfType<DropTableOperation>()
                .Select(table => table.Name).SequenceEqual(new[] { "ImportItems", "ImportRuns" }),
            "Down drops both tables, items first, and nothing else");

        var generator = context.GetService<IMigrationsSqlGenerator>();
        var sql = string.Join("\n", generator.Generate(up, model).Select(command => command.CommandText));
        check(sql.Contains("CREATE TABLE [ImportRuns]") && sql.Contains("CREATE TABLE [ImportItems]") &&
            sql.Contains("CREATE UNIQUE INDEX [IX_ImportItems_ImportRunId_SourceUrl] ON [ImportItems] ([ImportRunId], [SourceUrl])") &&
            sql.Contains("ON DELETE CASCADE") && sql.Contains("ON DELETE NO ACTION"),
            "SQL Server DDL creates both tables with their unique index and their delete behaviour");
        var downSql = string.Join("\n", generator.Generate(down, model).Select(command => command.CommandText));
        check(downSql.Contains("DROP TABLE [ImportItems]") && downSql.Contains("DROP TABLE [ImportRuns]") &&
            downSql.IndexOf("DROP TABLE [ImportItems]", StringComparison.Ordinal) <
            downSql.IndexOf("DROP TABLE [ImportRuns]", StringComparison.Ordinal),
            "SQL Server rollback drops the items before the runs");

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

    private static void PlannerContract(Action<bool, string> check)
    {
        var store = new StoreContext(Guid.NewGuid(), "Irion", "irion.cl", "shopify", "CLP", 20, 500);
        var start = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);
        const string first = "https://irion.cl/products/gundam-a";
        const string second = "https://irion.cl/products/gundam-b";
        const string third = "https://irion.cl/products/gundam-c";
        const string hidden = "https://irion.cl/products/oculto";
        var discovery = new CatalogDiscoveryResult(
            [first, second, third],
            [new CatalogSkip("https://irion.cl/sitemap_collections_1.xml", CatalogSkipReason.NotProductSitemap)],
            [new CatalogSkip(hidden, CatalogSkipReason.RobotsDisallowed)],
            4,
            20);

        var budget = new RequestBudget(500);
        var plan = ImportRunPlanner.Start(store, budget, discovery, new HashSet<string>(), start);
        check(plan.Run.StoreId == store.StoreId && plan.Run.RequestBudget == 500 &&
            plan.Run.Status == ImportRunStatus.Running && plan.Run.StartedAtUtc == start,
            "Starting a run records the store and the budget it was started with");
        check(plan.Items.Count == 4 &&
            plan.Items.Where(item => item.State == ImportItemState.Pending)
                .Select(item => item.SourceUrl).SequenceEqual(new[] { first, second, third }),
            "Every discovered product URL is queued once, in sitemap order");
        check(plan.Items.Single(item => item.SourceUrl == hidden) is
                { State: ImportItemState.Skipped, HttpStatus: null, AttemptedAtUtc: null } skippedItem &&
            skippedItem.ErrorMessage == CatalogSkip.RobotsDisallowedMessage,
            "A product URL rejected by robots is queued as skipped with its reason and no request");
        check(plan.Items.All(item => item.ImportRunId == plan.Run.ImportRunId),
            "Every item belongs to the run that queued it");
        check(plan.Run.DiscoveredCount == 3 &&
            plan.Run.DiscoveredCount == plan.Items.Count(item => item.State == ImportItemState.Pending) &&
            plan.Run.SkippedCount == 2 && plan.Run.StoredCount == 0 && plan.Run.FailedCount == 0,
            "The discovered counter is the queued items, not the entries the walk discarded");
        check(plan.Discovery == discovery, "The plan returns the discovery that produced it");

        // (c) Una segunda ejecución no vuelve a encolar lo que una ejecución completada ya guardó.
        var again = ImportRunPlanner.Start(store, new RequestBudget(500), discovery,
            new HashSet<string>([first], StringComparer.OrdinalIgnoreCase), start.AddHours(12));
        var repeated = again.Items.Where(item => item.SourceUrl == first).ToArray();
        check(repeated.Length == 1 && repeated[0].State == ImportItemState.Skipped &&
            repeated[0].ErrorMessage == CatalogSkip.AlreadyStoredMessage,
            "A stored URL is recorded as skipped instead of queued again");
        check(again.Items.Where(item => item.State == ImportItemState.Pending)
                .Select(item => item.SourceUrl).SequenceEqual(new[] { second, third }),
            "Only the URLs a previous run did not store are queued");
        check(again.Run.DiscoveredCount == 2 && again.Run.SkippedCount == 3 &&
            again.Run.DiscoveredCount == again.Items.Count(item => item.State == ImportItemState.Pending) &&
            again.Items.All(item => item.ImportRunId == again.Run.ImportRunId),
            "The second run counts only the items it really queued, never the URL a previous run stored");
        check(again.Run.ImportRunId != plan.Run.ImportRunId &&
            !again.Items.Any(item => plan.Items.Any(previous => previous.ImportItemId == item.ImportItemId)),
            "A second run never reuses the items of the previous one");

        var duplicated = ImportRunPlanner.Start(store, new RequestBudget(500),
            discovery with { ProductUrls = [first, first, second] }, new HashSet<string>(), start);
        check(duplicated.Items.Count(item => item.SourceUrl == first) == 1 &&
            duplicated.Items.Count(item => item.State == ImportItemState.Pending) == 2 &&
            duplicated.Run.DiscoveredCount == 2,
            "A parameterised sitemap that repeats a URL cannot duplicate an item or inflate the counter");

        // (d) Una reanudación sólo procesa los pendientes y los fallos transitorios.
        var at = start.AddMinutes(5);
        var resumable = plan.Items.ToArray();
        resumable.Single(item => item.SourceUrl == first).MarkStored(200, at);
        resumable.Single(item => item.SourceUrl == second).MarkFailed(500, "Servicio no disponible.", at);
        resumable.Single(item => item.SourceUrl == third).MarkSkipped(404, "La ficha ya no existe.", at);
        plan.Run.Fail(at);
        var work = ImportRunPlanner.Resume(plan.Run, resumable);
        check(work.Select(item => item.SourceUrl).SequenceEqual(new[] { second }),
            "Resuming processes only pending items and transient failures");
        check(plan.Run.Status == ImportRunStatus.Running && plan.Run.FinishedAtUtc is null &&
            plan.Run.StoredCount == 0 && plan.Run.FailedCount == 0,
            "Resuming reopens the run without inventing counters");
        check(ImportRunPlanner.Resume(plan.Run, resumable).Count == work.Count,
            "Resuming twice returns the same work, never new items");

        var ordered = ImportRunPlanner.Start(store, new RequestBudget(10),
            new CatalogDiscoveryResult(
                ["https://irion.cl/products/c", "https://irion.cl/products/a", "https://irion.cl/products/b"],
                [], [], 3, 4), new HashSet<string>(), start);
        check(ImportRunPlanner.Resume(ordered.Run, ordered.Items)
                .Select(item => item.SourceUrl)
                .SequenceEqual(new[]
                {
                    "https://irion.cl/products/a", "https://irion.cl/products/b", "https://irion.cl/products/c"
                }),
            "Resume returns a deterministic order for the same run");

        var foreign = ImportRunPlanner.Start(store, new RequestBudget(10),
            new CatalogDiscoveryResult(["https://irion.cl/products/z"], [], [], 1, 2), new HashSet<string>(), start);
        Reject(() => _ = ImportRunPlanner.Resume(foreign.Run, ordered.Items), ItemForeignRun);
    }

    private static ApplicationDbContext StructuralContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true")
            .Options);

    private static void Reject(Action action, string message) => RejectMessage(action, message);

    private static void RejectMessage(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (exception is System.ArgumentException or InvalidOperationException)
        {
            // ArgumentException añade el nombre del parámetro al mensaje; se compara el texto propio.
            var actual = exception.Message;
            var parameter = actual.IndexOf(" (Parameter '", StringComparison.Ordinal);
            if (parameter > 0) actual = actual[..parameter];
            if (actual == message) return;
            throw new Exception($"Unexpected rejection message: {actual}");
        }

        throw new Exception("Expected rejection: " + message);
    }
}
