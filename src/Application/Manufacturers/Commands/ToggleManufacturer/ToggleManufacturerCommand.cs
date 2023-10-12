using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Manufacturers.Commands.ToggleManufacturer;

public class ToggleManufacturerCommand : IRequest
{
    public int id { get; set; }

    public class ToggleManufacturerCommandHandler : IRequestHandler<ToggleManufacturerCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleManufacturerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleManufacturerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Manufacturers.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Manufacturer), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}

