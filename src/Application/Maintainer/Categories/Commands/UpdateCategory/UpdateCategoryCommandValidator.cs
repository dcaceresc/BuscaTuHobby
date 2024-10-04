using FluentValidation;

namespace Application.Maintainer.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategory>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(v => v.CategoryId).NotNull().NotEmpty();
        RuleFor(v => v.CategoryName).NotNull().NotEmpty();
    }
}