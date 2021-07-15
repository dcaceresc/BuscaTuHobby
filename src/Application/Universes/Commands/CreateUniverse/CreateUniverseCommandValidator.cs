using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Universes.Commands.CreateUniverse
{
    public class CreateUniverseCommandValidator : AbstractValidator<CreateUniverseCommand>
    {
        public CreateUniverseCommandValidator()
        {
            RuleFor(v => v.Name).NotNull().NotEmpty();
        }
    }
}
