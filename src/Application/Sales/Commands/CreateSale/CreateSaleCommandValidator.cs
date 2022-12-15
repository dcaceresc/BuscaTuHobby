using FluentValidation;

namespace Application.Sales.Commands.CreateSale;
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(v => v.gunplaId).NotNull();
        RuleFor(v => v.storeId).NotNull();
        RuleFor(v => v.price).NotNull();

    }
}

