using FluentValidation;

namespace Application.Maintainer.Inventories.Commands.CreateInventory;
public class CreateInventoryValidator : AbstractValidator<CreateInventory>
{
    public CreateInventoryValidator()
    {
        RuleFor(v => v.ProductId).NotNull();
        RuleFor(v => v.StoreId).NotNull();
        RuleFor(v => v.Price).NotNull();

    }
}

