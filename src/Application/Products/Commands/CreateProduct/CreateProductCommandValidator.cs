
using FluentValidation;

namespace Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.name).NotNull();
        RuleFor(v => v.gradeId).NotNull();
        RuleFor(v => v.scaleId).NotNull();
        RuleFor(v => v.manufacturerId).NotNull();
        RuleFor(v => v.serieId).NotNull();
        RuleFor(v => v.hasBase).NotNull();
        RuleFor(v => v.description).NotNull();
        RuleFor(v => v.releaseDate).NotNull();
    }
}