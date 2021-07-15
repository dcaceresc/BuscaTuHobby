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

namespace Application.Photos.Commands.DeletePhoto
{
    public class DeletePhotoCommand : IRequest
    {
        public int Id { get; set; }

        public class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand>
        {
            private readonly IApplicationDbContext _context;

            public DeletePhotoCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Photos.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Photo), request.Id);
                }

                _context.Photos.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
