using FluentValidation;

namespace Application.Inventories.Commands.CreateInventory;
public class CreateInventoryCommandValidator : AbstractValidator<CreateInventoryCommand>
{
    public CreateInventoryCommandValidator()
    {
        RuleFor(v => v.gunplaId).NotNull();
        RuleFor(v => v.storeId).NotNull();
        RuleFor(v => v.price).NotNull();

    }
}

