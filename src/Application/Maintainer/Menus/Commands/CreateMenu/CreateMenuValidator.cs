using FluentValidation;

namespace Application.Maintainer.Menus.Commands.CreateMenu;

public class CreateMenuValidator : AbstractValidator<CreateMenu>
{
    public CreateMenuValidator()
    {
        RuleFor(v => v.MenuName).NotNull().NotEmpty();
        RuleFor(v => v.MenuOrder).NotNull().NotEmpty();
    }
}

