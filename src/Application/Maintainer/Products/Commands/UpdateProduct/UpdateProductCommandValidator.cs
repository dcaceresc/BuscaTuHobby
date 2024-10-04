using FluentValidation;

namespace Application.Maintainer.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.ProductId).NotNull();
        RuleFor(v => v.ProductName).NotNull();
        RuleFor(v => v.ManufacturerId).NotNull();
        RuleFor(v => v.FranchiseId).NotNull();
        RuleFor(v => v.ProductHasBase).NotNull();
        RuleFor(v => v.ProductDescription).NotNull();
        RuleFor(v => v.ProductReleaseDate).NotNull();
    }
}