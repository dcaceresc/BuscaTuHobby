
using FluentValidation;

namespace Application.Inventories.Commands.UpdateInventory;


public class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
{
    public UpdateInventoryCommandValidator()
    {
        RuleFor(v => v.id).NotNull();
        RuleFor(v => v.productId).NotNull();
        RuleFor(v => v.storeId).NotNull();
        RuleFor(v => v.price).NotNull();
    }
}