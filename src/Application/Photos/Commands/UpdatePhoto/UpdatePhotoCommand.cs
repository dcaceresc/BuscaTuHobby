﻿using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Photos.Commands.UpdatePhoto;
public class UpdatePhotoCommand : IRequest
{
    public int id { get; set; }
    public int order { get; set; }
    public byte[] imageData { get; set; } = default!;
    public int productId { get; set; }

    public class UpdatePhotoCommandHandler : IRequestHandler<UpdatePhotoCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdatePhotoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdatePhotoCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Photos.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Photo), request.id);
            }

            entity.order = request.order;
            entity.imageData = request.imageData;
            entity.productId = request.productId;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
