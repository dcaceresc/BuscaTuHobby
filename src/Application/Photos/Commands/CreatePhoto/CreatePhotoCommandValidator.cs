using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Photos.Commands.CreatePhoto
{
    public class CreatePhotoCommandValidator : AbstractValidator<CreatePhotoCommand>
    {
        public CreatePhotoCommandValidator()
        {
            RuleFor(v => v.Order).NotNull();
            RuleFor(v => v.ImageData).NotNull();
            RuleFor(v => v.GunplaId).NotNull();
        }
    }
}
