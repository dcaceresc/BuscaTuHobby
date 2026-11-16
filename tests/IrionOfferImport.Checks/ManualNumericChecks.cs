using System.Reflection;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Infrastructure.Services;

internal static class ManualNumericChecks
{
    public static async Task Run(Action<bool, string> check, TextWriter? output = null)
    {
        var product = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var store = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var cases = new[] { (0, 0, 0), (100, 90, 0), (100, 0, 1), (100, 200, 101), (120, 150, 20) };
        foreach (var (price, original, discount) in cases)
        {
            var context = DispatchProxy.Create<IApplicationDbContext, ContextProxy>();
            var fake = (ContextProxy)(object)context;
            var created = await new CreateInventoryHandler(context, new ApiResponseService())
                .Handle(new(product, store, price, original, discount), default);
            var row = fake.Rows.SingleOrDefault();
            check(created.Success && created.Message == "Inventario creado correctamente" &&
                row is not null && row.ProductId == product && row.StoreId == store && row.Price == price &&
                row.OriginalPrice == original && row.DiscountPercentage == discount && row.IsActive &&
                fake.Saves == 1 && fake.Rows.Count == 1, $"Manual create preserves tuple ({price},{original},{discount})");
            output?.WriteLine(JsonSerializer.Serialize(new { Command = "create", InputPrice = price, InputOriginalPrice = original, InputDiscountPercentage = discount,
                created.Success, created.Message, row?.ProductId, row?.StoreId, row?.Price, row?.OriginalPrice,
                row?.DiscountPercentage, row?.IsActive, fake.Saves, Rows = fake.Rows.Count }));
            if (row is null) continue;
            row.Update(product, store, 999, 0, 0, row.Condition, row.PaymentMethodLabel);
            row.ToggleActive();
            var updated = await new UpdateInventoryHandler(context, new ApiResponseService()).Handle(new()
            {
                InventoryId = row.InventoryId, ProductId = product, StoreId = store,
                Price = price, OriginalPrice = original, DiscountPercentage = discount
            }, default);
            check(updated.Success && updated.Message == "Inventario actualizado correctamente" &&
                row.ProductId == product && row.StoreId == store && row.Price == price &&
                row.OriginalPrice == original && row.DiscountPercentage == discount && !row.IsActive &&
                fake.Saves == 2 && fake.Rows.Count == 1, $"Manual update preserves tuple ({price},{original},{discount})");
            output?.WriteLine(JsonSerializer.Serialize(new { Command = "update", InputPrice = price, InputOriginalPrice = original, InputDiscountPercentage = discount,
                updated.Success, updated.Message, row.ProductId, row.StoreId, row.Price, row.OriginalPrice,
                row.DiscountPercentage, row.IsActive, fake.Saves, Rows = fake.Rows.Count }));
        }
    }
}
