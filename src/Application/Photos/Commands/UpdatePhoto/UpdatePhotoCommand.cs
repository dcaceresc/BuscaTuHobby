using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Photos.Commands.UpdatePhoto
{
    public class UpdatePhotoCommand : IRequest
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public byte[] ImageData { get; set; }
        public int GunplaId { get; set; }

        public class UpdatePhotoCommandHandler : IRequestHandler<UpdatePhotoCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdatePhotoCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdatePhotoCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Photos.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Photo), request.Id);
                }

                entity.Order = request.Order;
                entity.ImageData = request.ImageData;
                entity.GunplaId = request.GunplaId;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
