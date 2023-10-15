using FluentValidation;

namespace Application.Inventories.Commands.CreateInventory;
public class CreateInventoryCommandValidator : AbstractValidator<CreateInventoryCommand>
{
    public CreateInventoryCommandValidator()
    {
        RuleFor(v => v.productId).NotNull();
        RuleFor(v => v.storeId).NotNull();
        RuleFor(v => v.price).NotNull();

    }
}

