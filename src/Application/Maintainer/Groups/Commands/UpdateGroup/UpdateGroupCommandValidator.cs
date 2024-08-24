using FluentValidation;

namespace Application.Maintainer.Groups.Commands.UpdateGroup;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroup>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(v => v.GroupId).NotNull().NotEmpty();
        RuleFor(v => v.GroupName).NotNull().NotEmpty();
    }
}

