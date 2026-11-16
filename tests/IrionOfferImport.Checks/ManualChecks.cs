using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

internal static class ManualChecks
{
    public static async Task Run(Action<bool, string> check, Guid product, Guid store)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
        var fake = (ContextProxy)(object)context;
        var create = new CreateInventoryHandler(context, new ApiResponseService());
        var update = new UpdateInventoryHandler(context, new ApiResponseService());
        var response = await create.Handle(new(product, store, 100, 0, 0), default);
        check(response.Success && response.Message == "Inventario creado correctamente" && fake.Saves == 1 && fake.Rows.Count == 1,
            "Existing manual creation succeeds");
        var row = fake.Rows.Single();
        row.ToggleActive();
        string State() => System.Text.Json.JsonSerializer.Serialize(fake.Rows);
        var before = State();
        response = await create.Handle(new(product, store, 200, 0, 0), default);
        check(!response.Success && response.Message == "No se pudo crear el inventario" && fake.Saves == 1 && fake.Rows.Count == 1 && State() == before,
            "Duplicate disabled pair rejected without mutation/save");
        var other = Inventory.Create(Guid.NewGuid(), store, 300, 0, 0);
        fake.Rows.Add(other);
        before = State();
        UpdateInventory Edit(Guid targetProduct, int price = 120) => new()
        { InventoryId = row.InventoryId, ProductId = targetProduct, StoreId = store, Price = price, OriginalPrice = 0, DiscountPercentage = 0 };
        response = await update.Handle(Edit(other.ProductId), default);
        check(!response.Success && response.Message == "No se pudo actualizar el inventario" && State() == before && fake.Saves == 1,
            "Manual pair collision rejected before mutation/save");
        response = await update.Handle(Edit(product), default);
        check(response.Success && response.Message == "Inventario actualizado correctamente" && row.Price == 120 && !row.IsActive && fake.Saves == 2,
            "Existing manual update succeeds and excludes self from collision");
        var newProduct = Guid.NewGuid();
        response = await update.Handle(Edit(newProduct), default);
        check(response.Success && row.ProductId == newProduct && !row.IsActive && fake.Saves == 3,
            "Unlinked manual row still permits association changes");
        fake.SaveError = new DbUpdateException("private SQL constraint detail");
        response = await create.Handle(new(Guid.NewGuid(), store, 100, 0, 0), default);
        check(!response.Success && response.Message == "No se pudo crear el inventario", "Unique persistence errors stay generic");
        fake.SaveError = new DbUpdateConcurrencyException("private concurrency detail");
        response = await update.Handle(Edit(newProduct), default);
        check(!response.Success && response.Message == "No se pudo actualizar el inventario", "Concurrency errors stay generic");
        fake.SaveError = null;
        var factory = typeof(Inventory).GetMethod("CreateImportedOffer");
        if (factory is not null)
        {
            var imported = (Inventory)factory.Invoke(null, [Guid.NewGuid(), store, 100, 0, 0,
                "https://irion.cl/products/model", "123", true, DateTime.UtcNow])!;
            fake.Rows.Add(imported);
            var saves = fake.Saves;
            before = State();
            response = await update.Handle(new() { InventoryId = imported.InventoryId, ProductId = Guid.NewGuid(), StoreId = store, Price = 150 }, default);
            check(!response.Success && response.Message == "No se pudo actualizar el inventario" && State() == before && fake.Saves == saves,
                "Imported association change rejected through public manual handler without mutation/save");
        }
    }
}

public class ContextProxy : DispatchProxy
{
    public List<Inventory> Rows { get; } = [];
    public List<InventoryPriceHistory> Histories { get; } = [];
    public int Saves { get; private set; }
    public Exception? SaveError { get; set; }
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod!.Name switch
    {
        "get_Inventories" => new InventorySet(Rows),
        "get_InventoryPriceHistories" => new InspectionSet<InventoryPriceHistory>(Histories, _ => { }),
        "SaveChangesAsync" => Save(),
        _ => throw new NotSupportedException(targetMethod.Name)
    };
    private Task<int> Save()
    {
        Saves++;
        return SaveError is null ? Task.FromResult(1) : Task.FromException<int>(SaveError);
    }
}

internal sealed class InventorySet(List<Inventory> rows) : DbSet<Inventory>, IQueryable<Inventory>
{
    public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => throw new NotSupportedException();
    private IQueryable<Inventory> Query => rows.AsQueryable();
    public override EntityEntry<Inventory> Add(Inventory entity) { rows.Add(entity); return null!; }
    public override ValueTask<Inventory?> FindAsync(object?[]? keys, CancellationToken cancellationToken) =>
        ValueTask.FromResult(rows.SingleOrDefault(i => i.InventoryId == (Guid)keys![0]!));
    Type IQueryable.ElementType => typeof(Inventory);
    Expression IQueryable.Expression => Query.Expression;
    IQueryProvider IQueryable.Provider => new AsyncProvider(Query.Provider);
    IEnumerator<Inventory> IEnumerable<Inventory>.GetEnumerator() => rows.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => rows.GetEnumerator();
}

internal sealed class AsyncProvider(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) => inner.CreateQuery(expression);
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => inner.CreateQuery<TElement>(expression);
    public object? Execute(Expression expression) => inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resultType = typeof(TResult).GetGenericArguments().Single();
        var result = inner.Execute(expression);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, [result])!;
    }
}
