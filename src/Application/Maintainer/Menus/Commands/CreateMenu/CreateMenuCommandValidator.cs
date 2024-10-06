using FluentValidation;

namespace Application.Maintainer.Menus.Commands.CreateMenu;

public class CreateMenuCommandValidator : AbstractValidator<CreateMenu>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(v => v.MenuName).NotNull().NotEmpty();
        RuleFor(v => v.MenuOrden).NotNull().NotEmpty();
    }
}

