using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Photos.Commands.UpdatePhoto
{
    public class UpdatePhotoCommandValidator : AbstractValidator<UpdatePhotoCommand>
    {
        public UpdatePhotoCommandValidator()
        {
            RuleFor(v => v.Id).NotNull();
            RuleFor(v => v.Order).NotNull();
            RuleFor(v => v.ImageData).NotNull();
            RuleFor(v => v.GunplaId).NotNull();
        }
    }
}
