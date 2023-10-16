using FluentValidation;

namespace Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(v => v.id).NotNull().NotEmpty();
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}

