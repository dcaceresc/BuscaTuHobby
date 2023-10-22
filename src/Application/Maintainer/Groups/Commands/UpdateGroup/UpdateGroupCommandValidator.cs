using FluentValidation;

namespace Application.Maintainer.Groups.Commands.UpdateGroup;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(v => v.id).NotNull().NotEmpty();
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}

