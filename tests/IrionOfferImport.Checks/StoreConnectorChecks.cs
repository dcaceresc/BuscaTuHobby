using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

// Ingesta multi-tienda: la identidad de la tienda es una fila de configuración, el conector es por plataforma y
// el cargador elige por la clave que declara esa fila. Todo con dobles en memoria: ni base de datos ni red.
internal static class StoreConnectorChecks
{
    private const string SettingsWithoutKey = "Los ajustes del conector requieren una clave de conector.";
    private const string LongKey = "La clave del conector no puede superar los 32 caracteres.";
    private const string LongSettings = "Los ajustes del conector no pueden superar los 4000 caracteres.";

    public static async Task Run(Action<bool, string> check)
    {
        EntityContract(check);
        ModelShape(check);
        MigrationShape(check);
        SettingsContract(check);
        await LoaderContract(check);
        DiContract(check);
    }

    private static void EntityContract(Action<bool, string> check)
    {
        var store = Store.Create("Irion", "https://irion.cl", "bi bi-shop", 0, "irion");
        check(store.ConnectorKey is null && store.ConnectorSettings is null,
            "A store without ingestion configuration declares no connector");

        store.ConfigureConnector("  shopify ", " {\"currency\":\"CLP\"} ");
        check(store.ConnectorKey == "shopify" && store.ConnectorSettings == "{\"currency\":\"CLP\"}",
            "The platform key and its settings are stored trimmed");

        store.ConfigureConnector("shopify", null);
        check(store.ConnectorKey == "shopify" && store.ConnectorSettings is null,
            "Declaring the platform does not require settings; the defaults apply later");

        store.ConfigureConnector("shopify", "{\"currency\":\"CLP\"}");
        var key = store.ConnectorKey;
        var settings = store.ConnectorSettings;
        Reject(() => store.ConfigureConnector(null, "{\"currency\":\"CLP\"}"), SettingsWithoutKey);
        check(store.ConnectorKey == key && store.ConnectorSettings == settings,
            "Settings without a platform are rejected and the row is left untouched");
        Reject(() => store.ConfigureConnector("shopify", new string('s', 4001)), LongSettings);
        Reject(() => store.ConfigureConnector(new string('k', 33), null), LongKey);
        check(store.ConnectorKey == key && store.ConnectorSettings == settings,
            "Rejected configuration never leaves the row half written");

        store.ConfigureConnector(new string('k', 32), new string('s', 4000));
        check(store.ConnectorKey?.Length == 32 && store.ConnectorSettings?.Length == 4000,
            "Both column limits are accepted at their boundary");

        store.ConfigureConnector("   ", null);
        check(store.ConnectorKey is null && store.ConnectorSettings is null,
            "An empty key clears the ingestion configuration of the store");
    }

    private static void ModelShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var store = model.FindEntityType(typeof(Store))!;
        var key = store.FindProperty("ConnectorKey");
        var settings = store.FindProperty("ConnectorSettings");
        check(key is not null && key.IsNullable && key.GetMaxLength() == 32 &&
            key.GetColumnType() == "nvarchar(32)",
            "The model maps the connector key as nullable nvarchar(32)");
        check(settings is not null && settings.IsNullable && settings.GetMaxLength() == 4000 &&
            settings.GetColumnType() == "nvarchar(4000)",
            "The model maps the connector settings as nullable nvarchar(4000)");
    }

    private static void MigrationShape(Action<bool, string> check)
    {
        using var context = StructuralContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var migrations = context.GetService<IMigrationsAssembly>();
        var entry = migrations.Migrations.OrderBy(m => m.Key, StringComparer.Ordinal)
            .LastOrDefault(m => m.Key.EndsWith("_StoreConnectorConfiguration"));
        check(entry.Value is not null &&
            entry.Key == "20261116000000_StoreConnectorConfiguration" &&
            string.CompareOrdinal(entry.Key, "20260723000000_ProductGradeScaleAndPartNumber") > 0,
            "The connector configuration migration is discoverable and sorts after the Gunpla migration");
        if (entry.Value is null) return;

        var migration = migrations.CreateMigration(entry.Value, context.Database.ProviderName!);
        var up = migration.UpOperations;
        var added = up.OfType<AddColumnOperation>().OrderBy(c => c.Name, StringComparer.Ordinal).ToArray();
        check(up.Count == 3 && added.Length == 2 && added.All(c => c.Table == "Stores" && c.IsNullable),
            "Up only adds the two nullable connector columns to Stores");
        check(added[0] is { Name: "ConnectorKey", ColumnType: "nvarchar(32)", MaxLength: 32 } &&
            added[1] is { Name: "ConnectorSettings", ColumnType: "nvarchar(4000)", MaxLength: 4000 },
            "Up declares the exact types and lengths of the two columns");

        var backfill = up.OfType<SqlOperation>().SingleOrDefault();
        check(backfill is not null && backfill.Sql.Contains("UPDATE [Stores]") &&
            backfill.Sql.Contains("[ConnectorKey] = 'shopify'") && backfill.Sql.Contains("irion.cl") &&
            backfill.Sql.Contains("IS NULL"),
            "Up configures the existing Irion row so its manual import keeps working without hand editing");

        var down = migration.DownOperations;
        check(down.Count == 2 && down.OfType<DropColumnOperation>()
                .Select(c => c.Table + "." + c.Name).OrderBy(n => n, StringComparer.Ordinal)
                .SequenceEqual(new[] { "Stores.ConnectorKey", "Stores.ConnectorSettings" }),
            "Down drops exactly the two columns and nothing else");

        var generator = context.GetService<IMigrationsSqlGenerator>();
        var sql = string.Join("\n", generator.Generate(up, model).Select(c => c.CommandText));
        check(sql.Contains("ALTER TABLE [Stores] ADD [ConnectorKey] nvarchar(32) NULL") &&
            sql.Contains("ALTER TABLE [Stores] ADD [ConnectorSettings] nvarchar(4000) NULL") &&
            sql.Contains("UPDATE [Stores]"),
            "SQL Server DDL adds both nullable columns before the backfill");
        var downSql = string.Join("\n", generator.Generate(down, model).Select(c => c.CommandText));
        check(downSql.Contains("DROP COLUMN [ConnectorKey]") &&
            downSql.Contains("DROP COLUMN [ConnectorSettings]"),
            "SQL Server rollback drops both columns");

        var initializer = context.GetService<IModelRuntimeInitializer>();
        var snapshot = initializer.Initialize(migrations.ModelSnapshot!.Model, designTime: true);
        var differ = context.GetService<IMigrationsModelDiffer>();
        check(!differ.HasDifferences(snapshot.GetRelationalModel(), model.GetRelationalModel()),
            "The prepared snapshot matches the current model");
    }

    private static void SettingsContract(Action<bool, string> check)
    {
        var defaults = StoreConnectorSettings.Parse(null, "Irion");
        check(defaults == new StoreConnectorSettings("CLP", 20, 500),
            "An unconfigured store keeps the conservative defaults");
        var declared = StoreConnectorSettings.Parse(
            "{\"currency\":\"usd\",\"requestsPerMinute\":5,\"requestBudget\":50}", "LuffyToys");
        check(declared == new StoreConnectorSettings("USD", 5, 50),
            "Per-store overrides win over the defaults, including the request budget");
        RejectConfiguration(() => StoreConnectorSettings.Parse("{", "LuffyToys"),
            "Los ajustes del conector de LuffyToys no son JSON válido.");
        RejectConfiguration(() => StoreConnectorSettings.Parse("[]", "LuffyToys"),
            "Los ajustes del conector de LuffyToys deben ser un objeto JSON.");
        RejectConfiguration(() => StoreConnectorSettings.Parse("{\"currency\":\"CL\"}", "LuffyToys"),
            "La moneda configurada para LuffyToys no es un código ISO de tres letras.");
        RejectConfiguration(() => StoreConnectorSettings.Parse("{\"requestBudget\":0}", "LuffyToys"),
            "El ajuste requestBudget de LuffyToys debe ser un entero positivo.");
        RejectConfiguration(() => StoreConnectorSettings.Parse("{\"requestsPerMinute\":\"fast\"}", "LuffyToys"),
            "El ajuste requestsPerMinute de LuffyToys debe ser un entero positivo.");
    }

    private static async Task LoaderContract(Action<bool, string> check)
    {
        var irion = new StoreContext(Guid.NewGuid(), "Irion", "irion.cl", "shopify", "CLP", 20, 500);
        var luffy = new StoreContext(Guid.NewGuid(), "LuffyToys", "luffytoys.cl", "shopify", "USD", 10, 200);
        var provider = new RecordingProvider(irion, luffy);
        var connector = new RecordingConnector("shopify");
        var loader = new StoreProductLoader(provider, [connector]);

        var luffyOffer = await loader.LoadAsync("https://www.luffytoys.cl/products/gundam-rx-78", default);
        check(connector.Calls == 1 && provider.Hosts.SequenceEqual(new[] { "luffytoys.cl" }) &&
            connector.Last == luffy && luffyOffer.Currency == "USD" &&
            luffyOffer.SourceUrl == "https://www.luffytoys.cl/products/gundam-rx-78",
            "A second configured row reuses the same connector and its own currency");

        var irionOffer = await loader.LoadAsync("https://irion.cl/products/optimus-prime", default);
        check(connector.Calls == 2 && connector.Last == irion && irionOffer.Currency == "CLP",
            "The Irion row is served by the same connector, not a special case");

        var calls = connector.Calls;
        var lookups = provider.Calls;
        await RejectAsync(() => loader.LoadAsync("https://unknown.test/products/x", default),
            StoreOfferUrlException.DefaultMessage);
        check(connector.Calls == calls && provider.Calls == lookups + 1,
            "An unknown host is rejected explicitly and never reaches a connector");
        foreach (var invalid in new[] { "", "http://irion.cl/products/x", "https://user@irion.cl/products/x",
            "https://irion.cl:444/products/x", "not a url" })
        {
            await RejectAsync(() => loader.LoadAsync(invalid, default), StoreOfferUrlException.DefaultMessage);
            check(provider.Calls == lookups + 1 && connector.Calls == calls,
                "An invalid URL is rejected before any store lookup: " + invalid);
        }

        var unconfigured = new StoreProductLoader(
            new RecordingProvider(irion with { ConnectorKey = "json-ld" }), [connector]);
        await RejectAsync(() => unconfigured.LoadAsync("https://irion.cl/products/a", default),
            "La tienda Irion declara un conector desconocido: json-ld.");
        check(connector.Calls == calls, "An unknown connector key never reaches a connector");
    }

    private static void DiContract(Action<bool, string> check)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        // El interceptor de auditoría lo registra la capa web; aquí basta un doble para resolver el contexto.
        services.AddScoped<IAuthenticationService, AnonymousAuthentication>();
        services.AddInfrastructure(new CheckConfiguration
        {
            ["ConnectionStrings:BuscaTuHobby"] =
                "Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true",
            ["Jwt:Issuer"] = "checks",
            ["Jwt:Audience"] = "checks",
            ["Jwt:Key"] = new string('k', 32)
        });

        using var provider = services.BuildServiceProvider();
        var connectors = provider.GetServices<IStoreConnector>().ToArray();
        check(connectors.Length == 1 && connectors[0].Key == "shopify",
            "The composition registers one Shopify connector shared by every configured store");
        using var scope = provider.CreateScope();
        check(scope.ServiceProvider.GetRequiredService<IStoreProductLoader>() is not null &&
            scope.ServiceProvider.GetRequiredService<IStoreContextProvider>() is not null,
            "The loader and the store configuration provider resolve from the container");
        check(scope.ServiceProvider.GetRequiredService<ICatalogDiscovery>() is ICatalogDiscovery &&
            scope.ServiceProvider.GetRequiredService<IImportRunService>() is IImportRunService,
            "Sitemap discovery and the run service resolve from the same composition");
        check(scope.ServiceProvider.GetRequiredService<IImportRunProcessor>() is IImportRunProcessor,
            "The run processor resolves with the same connector set");
    }

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
        catch (System.ArgumentException)
        {
            return;
        }

        throw new Exception("Expected rejection: " + message);
    }

    private static void RejectConfiguration(Action action, string message)
    {
        try { action(); }
        catch (StoreConfigurationException exception)
        {
            if (exception.Message == message) return;
            throw new Exception($"Unexpected message: {exception.Message}");
        }
        throw new Exception("Expected configuration rejection: " + message);
    }

    private static async Task RejectAsync(Func<Task> action, string message)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is StoreOfferUrlException or StoreConfigurationException)
        {
            if (exception.Message == message) return;
            throw new Exception($"Unexpected message: {exception.Message}");
        }
        throw new Exception("Expected rejection: " + message);
    }

    private sealed class RecordingProvider(params StoreContext[] stores) : IStoreContextProvider
    {
        public int Calls { get; private set; }
        public List<string> Hosts { get; } = [];

        public Task<StoreContext?> FindByHostAsync(string host, CancellationToken cancellationToken)
        {
            Calls++;
            Hosts.Add(host);
            return Task.FromResult(stores.SingleOrDefault(store => store.Host == host));
        }

        // El disparo manual y la programación los ejercita AutomationChecks con el proveedor real; este doble sólo
        // resuelve lo que la importación manual necesita.
        public Task<StoreContext?> FindBySlugAsync(string storeSlug, CancellationToken cancellationToken) =>
            Task.FromResult<StoreContext?>(null);

        public Task<IReadOnlyList<StoreContext>> ListActiveAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<StoreContext>>(stores);
    }

    private sealed class RecordingConnector(string key) : IStoreConnector
    {
        public int Calls { get; private set; }
        public StoreContext? Last { get; private set; }

        public string Key => key;

        public Task<IrionProductPreviewDto> LoadProductAsync(
            StoreContext store, string url, CancellationToken cancellationToken)
        {
            Calls++;
            Last = store;
            var offer = new IrionProductPreviewDto(url, "Model", "Vendor", store.Currency, "", [], [], true,
                [new IrionVariantPreviewDto("1", null, null, true, 10, null, null)]);
            return Task.FromResult(offer);
        }
    }

    private sealed class AnonymousAuthentication : IAuthenticationService
    {
        public string? UserName => "checks";

        public string CreateAccessToken(string username, IList<string> roles) => "unused";

        public Task AuthenticationLOGMOD(string userName, string password) => Task.CompletedTask;
    }

    private sealed class CheckConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string?> _values = new(StringComparer.OrdinalIgnoreCase);

        public string? this[string key]
        {
            get => _values.GetValueOrDefault(key);
            set => _values[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];

        public IChangeToken GetReloadToken() => new NeverChangingToken();

        public IConfigurationSection GetSection(string key) => new CheckSection(this, key, key);
    }

    private sealed class CheckSection(CheckConfiguration root, string key, string path) : IConfigurationSection
    {
        public string? this[string child]
        {
            get => root[path + ":" + child];
            set => root[path + ":" + child] = value;
        }

        public string Key => key;
        public string Path => path;

        public string? Value
        {
            get => root[path];
            set => root[path] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => root.GetReloadToken();
        public IConfigurationSection GetSection(string child) => new CheckSection(root, child, path + ":" + child);
    }

    private sealed class NeverChangingToken : IChangeToken
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => new Noop();

        private sealed class Noop : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
