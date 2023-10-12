using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Manufacturers.Commands.UpdateManufacturer;

public class UpdateManufacturerCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; }

    public class UpdateManufacturerCommandHandler : IRequestHandler<UpdateManufacturerCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateManufacturerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateManufacturerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Manufacturers.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Manufacturer), request.id);
            }

            entity.name = request.name;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}

