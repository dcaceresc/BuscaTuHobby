using FluentValidation;

namespace Application.Maintainer.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategory>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(v => v.CategoryName).NotNull().NotEmpty();
    }
}
