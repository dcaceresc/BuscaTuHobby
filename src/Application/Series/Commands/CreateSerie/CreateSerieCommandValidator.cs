using FluentValidation;

namespace Application.Series.Commands.CreateSerie
{
    public class CreateSerieCommandValidator : AbstractValidator<CreateSerieCommand>
    {
        public CreateSerieCommandValidator()
        {
            RuleFor(v => v.name).NotNull().NotEmpty();
            RuleFor(v => v.universeId).NotNull().NotEmpty();
        }
    }
}
