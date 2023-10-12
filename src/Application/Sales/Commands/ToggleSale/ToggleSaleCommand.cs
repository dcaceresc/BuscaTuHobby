using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Sales.Commands.ToggleSale;

public class ToggleSaleCommand : IRequest
{
    public int id { get; set; }

    public class ToggleSaleCommandHandler : IRequestHandler<ToggleSaleCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleSaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleSaleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Sales.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Sale), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
