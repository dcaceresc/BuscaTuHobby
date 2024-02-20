using FluentValidation;

namespace Application.Maintainer.Groups.Commands.UpdateGroup;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(v => v.GroupId).NotNull().NotEmpty();
        RuleFor(v => v.GroupName).NotNull().NotEmpty();
    }
}

