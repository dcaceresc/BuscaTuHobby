using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Series.Commands.CreateSerie
{
    public class CreateSerieCommandValidator : AbstractValidator<CreateSerieCommand>
    {
        public CreateSerieCommandValidator()
        {
            RuleFor(v => v.Name).NotNull().NotEmpty();
            RuleFor(v => v.UniverseId).NotNull().NotEmpty();
        }
    }
}
