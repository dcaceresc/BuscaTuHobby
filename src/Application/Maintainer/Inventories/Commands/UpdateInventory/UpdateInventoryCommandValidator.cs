using FluentValidation;

namespace Application.Maintainer.Inventories.Commands.UpdateInventory;


public class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
{
    public UpdateInventoryCommandValidator()
    {
        RuleFor(v => v.InventoryId).NotNull();
        RuleFor(v => v.ProductId).NotNull();
        RuleFor(v => v.StoreId).NotNull();
        RuleFor(v => v.Price).NotNull();
    }
}