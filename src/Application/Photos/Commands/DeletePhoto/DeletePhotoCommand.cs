﻿using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Photos.Commands.DeletePhoto;
public class DeletePhotoCommand : IRequest
{
    public int id { get; set; }

    public class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeletePhotoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Photos.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Photo), request.id);
            }

            _context.Photos.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
