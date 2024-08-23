using FluentValidation;

namespace Application.Maintainer.Inventories.Commands.CreateInventory;
public class CreateInventoryCommandValidator : AbstractValidator<CreateInventory>
{
    public CreateInventoryCommandValidator()
    {
        RuleFor(v => v.ProductId).NotNull();
        RuleFor(v => v.StoreId).NotNull();
        RuleFor(v => v.Price).NotNull();

    }
}

