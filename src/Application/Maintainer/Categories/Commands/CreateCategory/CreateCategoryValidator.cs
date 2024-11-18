using FluentValidation;

namespace Application.Maintainer.Categories.Commands.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategory>
{
    public CreateCategoryValidator()
    {
        RuleFor(v => v.CategoryName).NotNull().NotEmpty();
    }
}
