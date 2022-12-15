using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Manufacturers.Commands.CreateManufacturer;

public class CreateManufacturerCommand : IRequest<int>
{
    public string name { get; set; }

    public class CreateManufacturerCommandHandler : IRequestHandler<CreateManufacturerCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateManufacturerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateManufacturerCommand request, CancellationToken cancellationToken)
        {
            var entity = new Manufacturer
            {
                name = request.name
            };

            _context.Manufacturers.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }
}

