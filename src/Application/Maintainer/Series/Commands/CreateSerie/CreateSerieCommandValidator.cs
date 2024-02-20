using FluentValidation;

namespace Application.Maintainer.Series.Commands.CreateSerie
{
    public class CreateSerieCommandValidator : AbstractValidator<CreateSerieCommand>
    {
        public CreateSerieCommandValidator()
        {
            RuleFor(v => v.SerieName).NotNull().NotEmpty();
        }
    }
}
