using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Photos.Commands.CreatePhoto
{
    public class CreatePhotoCommand : IRequest<int>
    {
        public int Order { get; set; }
        public byte[] ImageData { get; set; }
        public int GunplaId { get; set; }

        public class CreatePhotoCommandHandler : IRequestHandler<CreatePhotoCommand, int>
        {
            private readonly IApplicationDbContext _context;

            public CreatePhotoCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<int> Handle(CreatePhotoCommand request, CancellationToken cancellationToken)
            {
                var entity = new Photo
                {
                    Order = request.Order,
                    ImageData = request.ImageData,
                    GunplaId = request.GunplaId
                };

                _context.Photos.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return entity.Id;
            }
        }
    }
}
