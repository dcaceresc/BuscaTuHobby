using FluentValidation;

namespace Application.Maintainer.Menus.Commands.UpdateMenu;

public class UpdateMenuCommandValidator : AbstractValidator<UpdateMenu>
{
    public UpdateMenuCommandValidator()
    {
        RuleFor(v => v.MenuId).NotNull().NotEmpty();
        RuleFor(v => v.MenuName).NotNull().NotEmpty();
    }
}

