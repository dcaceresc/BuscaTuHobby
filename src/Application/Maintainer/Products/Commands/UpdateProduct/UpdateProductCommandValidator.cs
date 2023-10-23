using FluentValidation;

namespace Application.Maintainer.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.id).NotNull();
        RuleFor(v => v.name).NotNull();
        RuleFor(v => v.scaleId).NotNull();
        RuleFor(v => v.manufacturerId).NotNull();
        RuleFor(v => v.franchiseId).NotNull();
        RuleFor(v => v.hasBase).NotNull();
        RuleFor(v => v.description).NotNull();
        RuleFor(v => v.releaseDate).NotNull();
    }
}