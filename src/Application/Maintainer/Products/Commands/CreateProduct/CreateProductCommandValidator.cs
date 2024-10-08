﻿using FluentValidation;

namespace Application.Maintainer.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProduct>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.ProductName).NotNull();
        RuleFor(v => v.ManufacturerId).NotNull();
        RuleFor(v => v.FranchiseId).NotNull();
        RuleFor(v => v.ProductHasBase).NotNull();
        RuleFor(v => v.ProductDescription).NotNull();
        RuleFor(v => v.ProductReleaseDate).NotNull();
    }
}