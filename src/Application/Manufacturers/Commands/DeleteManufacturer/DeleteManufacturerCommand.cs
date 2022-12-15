using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Manufacturers.Commands.DeleteManufacturer;

public class DeleteManufacturerCommand : IRequest
{
    public int id { get; set; }

    public class DeleteManufacturerCommandHandler : IRequestHandler<DeleteManufacturerCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteManufacturerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteManufacturerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Manufacturers.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Manufacturer), request.id);
            }

            _context.Manufacturers.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

