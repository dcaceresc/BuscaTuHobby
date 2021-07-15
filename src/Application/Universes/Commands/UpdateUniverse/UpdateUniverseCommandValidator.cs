﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Universes.Commands.UpdateUniverse
{
    public class UpdateUniverseCommandValidator : AbstractValidator<UpdateUniverseCommand>
    {
        public UpdateUniverseCommandValidator()
        {
            RuleFor(v => v.Id).NotNull().NotEmpty();
            RuleFor(v => v.Name).NotNull().NotEmpty();
        }
    }
}
