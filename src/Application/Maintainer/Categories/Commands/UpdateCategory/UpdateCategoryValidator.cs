using FluentValidation;

namespace Application.Maintainer.Categories.Commands.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategory>
{
    public UpdateCategoryValidator()
    {
        RuleFor(v => v.CategoryId).NotNull().NotEmpty();
        RuleFor(v => v.CategoryName).NotNull().NotEmpty();
    }
}