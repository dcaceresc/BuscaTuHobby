using FluentValidation;

namespace Application.Maintainer.Inventories.Commands.UpdateInventory;


public class UpdateInventoryValidator : AbstractValidator<UpdateInventory>
{
    public UpdateInventoryValidator()
    {
        RuleFor(v => v.InventoryId).NotNull();
        RuleFor(v => v.ProductId).NotNull();
        RuleFor(v => v.StoreId).NotNull();
        RuleFor(v => v.Price).NotNull();
    }
}