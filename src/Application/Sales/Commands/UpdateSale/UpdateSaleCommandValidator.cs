
using FluentValidation;

namespace Application.Sales.Commands.UpdateSale;


public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(v => v.id).NotNull();
        RuleFor(v => v.gunplaId).NotNull();
        RuleFor(v => v.storeId).NotNull();
        RuleFor(v => v.price).NotNull();
    }
}