using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Queries.InspectIrionOffer;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

internal static class InspectionChecks
{
    public static async Task Run(Action<bool, string> check)
    {
        const string source = "https://irion.cl/products/model-1";
        const string invalid = "Los datos de la oferta de Irion no son válidos.";
        const string productError = "Selecciona un producto activo existente.";
        const string storeError = "Selecciona una tienda activa de Irion con una URL válida.";
        const string conflict = "La oferta existente no está vinculada a esta URL y variante de Irion.";
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;
        var product = Product.Create("Model", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
        var store = Store.Create("Not a domain assertion", "https://irion.cl/", "", 0, "irion");
        fake.Products.Add(product);
        fake.Stores.Add(store);
        var handler = new InspectIrionOfferHandler(context, new ApiResponseService());
        var request = new InspectIrionOffer(product.ProductId, store.StoreId, source, "123");
        string State() => JsonSerializer.Serialize(new { fake.Products, fake.Stores, fake.Rows });

        async Task Inspect(InspectIrionOffer input, string? outcome, string? message = null, int queries = 1)
        {
            var before = State();
            var lookups = fake.PairLookups;
            var saves = fake.Saves;
            var result = await handler.Handle(input, default);
            check(result.Success == (outcome is not null) && result.Message == (outcome is null ? message : null),
                "Inspection exact envelope: " + (message ?? outcome));
            if (outcome is null)
                check(result.Data is null, "Invalid inspection has no data");
            else
            {
                var row = fake.Rows.SingleOrDefault();
                check(result.Data is { } data && data.Outcome == outcome && data.ProductId == input.ProductId &&
                    data.StoreId == input.StoreId && data.SourceUrl == input.SourceUrl && data.ExternalVariantId == input.ExternalVariantId &&
                    data.InventoryId == row?.InventoryId && data.RowVersion == (row is null ? null : Convert.ToBase64String(row.RowVersion)) &&
                    data.IsActive == row?.IsActive && data.Message == message,
                    "Exact typed inspection result: " + outcome);
            }
            check(State() == before && fake.Saves == saves && fake.PairLookups - lookups == queries,
                "All stored fields/counters unchanged; exact pair lookup count");
        }

        await Inspect(request, "Create");
        await Inspect(request with { ExternalVariantId = "9223372036854775807" }, "Create");
        foreach (var url in new[] { "https://irion.cl", "https://irion.cl:443/", "https://IRION.cl/" })
        {
            store.Update("Irion", url, "", 0, "irion");
            await Inspect(request, "Create");
        }
        store.Update("Irion", "https://irion.cl/", "", 0, "irion");
        var linked = Inventory.CreateImportedOffer(product.ProductId, store.StoreId, 100, 0, 0, source, "123", false, DateTime.UtcNow);
        typeof(Inventory).GetProperty(nameof(Inventory.RowVersion))!.SetValue(linked, new byte[] { 0, 1, 2, 255 });
        fake.Rows.Add(linked);
        await Inspect(request, "Update");
        linked.ToggleActive();
        await Inspect(request, "Update", "El inventario está deshabilitado y permanecerá deshabilitado.");
        await Inspect(request with { ExternalVariantId = "124" }, "Conflict", conflict);
        await Inspect(request with { SourceUrl = source + "-other" }, "Conflict", conflict);
        fake.Rows.Clear();
        fake.Rows.Add(Inventory.Create(product.ProductId, store.StoreId, 100, 0, 0));
        await Inspect(request, "Conflict", conflict);
        fake.Rows.Single().ToggleActive();
        await Inspect(request, "Conflict", conflict);

        foreach (var input in new[] { request with { ProductId = Guid.Empty }, request with { StoreId = Guid.Empty } })
            await Inspect(input, null, invalid, 0);
        await Inspect(request with { ProductId = Guid.NewGuid() }, null, productError, 0);
        product.ToggleActive();
        await Inspect(request, null, productError, 0);
        product.ToggleActive();
        await Inspect(request with { StoreId = Guid.NewGuid() }, null, storeError, 0);
        store.ToggleActive();
        await Inspect(request, null, storeError, 0);
        store.ToggleActive();
        foreach (var url in new[] { "https://other.cl/", "https://irion.cl.evil.test/", "https://fakeirion.cl/", "https://www.irion.cl/",
            "http://irion.cl/", "https://user@irion.cl/", "https://irion.cl:444/", "https://irion.cl/products/model",
            "https://irion.cl/?x=1", "https://irion.cl/#x", "https://irion.cl/../", " https://irion.cl/", "https://irion.cl/%2f", "not a URL" })
        {
            store.Update("Irion", url, "", 0, "irion");
            await Inspect(request, null, storeError, 0);
        }
        store.Update("Irion", "https://irion.cl/", "", 0, "irion");
        foreach (var url in new[] { null, "", "http://irion.cl/products/model", "https://other.cl/products/model", "https://www.irion.cl/products/model",
            "https://irion.cl:444/products/model", "https://user@irion.cl/products/model", source + "?x=1", source + "#x", source + "/",
            "https://irion.cl/products/m%6fdel", "https://irion.cl/products/../products/model", "https://irion.cl/products/-model",
            "https://irion.cl/products/model--1", "https://irion.cl/products/modél", "https://irion.cl/products/model_1",
            "https://irion.cl/collections/all/products/model", "https://irion.cl/products/model.js", " " + source, source + "\n" })
            await Inspect(request with { SourceUrl = url! }, null, invalid, 0);
        foreach (var variant in new[] { null, "", "0", "-1", "+1", "1.0", " 123", "123 ", "1e3", "１２３", "9223372036854775808" })
            await Inspect(request with { ExternalVariantId = variant! }, null, invalid, 0);

        var beforeCancel = State();
        var beforeLookups = fake.PairLookups;
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        try { await handler.Handle(request, canceled.Token); check(false, "Cancellation must propagate"); }
        catch (OperationCanceledException) { check(true, "Cancellation propagates"); }
        check(State() == beforeCancel && fake.Saves == 0 && fake.PairLookups == beforeLookups, "Canceled inspection has no effects");
        using var duringQuery = new CancellationTokenSource();
        var forwarded = 0;
        fake.QueryHook = token =>
        {
            check(token == duringQuery.Token, "Cancellation token forwarded to every query");
            if (++forwarded == 3) duringQuery.Cancel();
        };
        try { await handler.Handle(request, duringQuery.Token); check(false, "Query cancellation must propagate"); }
        catch (OperationCanceledException) { check(true, "Query cancellation propagates without an error envelope"); }
        check(forwarded == 3 && fake.PairLookups == beforeLookups + 1 && State() == beforeCancel && fake.Saves == 0,
            "Cancellation during pair lookup preserves stored rows and save count");
        fake.QueryHook = null;
        fake.QueryError = new InvalidOperationException("private database details");
        await Inspect(request, null, "No se pudo inspeccionar la oferta de Irion.", 0);
        check(typeof(InspectIrionOfferHandler).GetConstructors().Single().GetParameters().All(p =>
            p.ParameterType == typeof(IApplicationDbContext) || p.ParameterType == typeof(IApiResponseService)),
            "Inspection has no extractor/HTTP dependency");
    }
}

public class InspectionContext : DispatchProxy
{
    public List<Product> Products { get; } = [];
    public List<Store> Stores { get; } = [];
    public List<Inventory> Rows { get; } = [];
    public List<InventoryPriceHistory> Histories { get; } = [];
    public int PairLookups { get; private set; }
    public int Saves { get; private set; }
    public Exception? SaveError { get; set; }
    public Action<CancellationToken>? SaveHook { get; set; }
    public string Persisted { get; private set; } = "[]";
    public void CapturePersisted() => Persisted = JsonSerializer.Serialize(Rows);
    public Exception? QueryError { get; set; }
    public Action<CancellationToken>? QueryHook { get; set; }
    protected override object? Invoke(MethodInfo? method, object?[]? args) => method!.Name switch
    {
        "get_Products" => new InspectionSet<Product>(Products, OnQuery),
        "get_Stores" => new InspectionSet<Store>(Stores, OnQuery),
        "get_Inventories" => new InspectionSet<Inventory>(Rows, token => { PairLookups++; OnQuery(token); }),
        "get_InventoryPriceHistories" => new InspectionSet<InventoryPriceHistory>(Histories, OnQuery),
        "SaveChangesAsync" => Save((CancellationToken)args![0]!),
        _ => throw new NotSupportedException(method.Name)
    };
    private void OnQuery(CancellationToken token)
    {
        QueryHook?.Invoke(token);
        token.ThrowIfCancellationRequested();
        if (QueryError is not null) throw QueryError;
    }
    private Task<int> Save(CancellationToken token)
    {
        Saves++;
        SaveHook?.Invoke(token);
        token.ThrowIfCancellationRequested();
        if (SaveError is not null) return Task.FromException<int>(SaveError);
        CapturePersisted();
        return Task.FromResult(1);
    }
}

internal sealed class InspectionSet<T>(List<T> rows, Action<CancellationToken> onQuery) : DbSet<T>, IQueryable<T> where T : class
{
    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => throw new NotSupportedException();
    public override Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T> Add(T entity)
    {
        rows.Add(entity);
        return null!;
    }
    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => rows.AsQueryable().Expression;
    IQueryProvider IQueryable.Provider => new InspectionProvider(rows.AsQueryable().Provider, onQuery);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => rows.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => rows.GetEnumerator();
}

internal sealed class InspectionQuery<T>(IQueryable<T> query, Action<CancellationToken> onQuery)
    : IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    public Type ElementType => typeof(T);
    public Expression Expression => query.Expression;
    public IQueryProvider Provider => new InspectionProvider(query.Provider, onQuery);
    public IEnumerator<T> GetEnumerator() => query.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        onQuery(cancellationToken);
        foreach (var item in query)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
        await Task.CompletedTask;
    }
}

internal sealed class InspectionProvider(IQueryProvider inner, Action<CancellationToken> onQuery) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) => inner.CreateQuery(expression);
    public IQueryable<T> CreateQuery<T>(Expression expression) => new InspectionQuery<T>(inner.CreateQuery<T>(expression), onQuery);
    public object? Execute(Expression expression) => inner.Execute(expression);
    public T Execute<T>(Expression expression) => inner.Execute<T>(expression);
    public T ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken = default)
    {
        onQuery(cancellationToken);
        var type = typeof(T).GetGenericArguments().Single();
        return (T)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(type).Invoke(null, [inner.Execute(expression)])!;
    }
}
