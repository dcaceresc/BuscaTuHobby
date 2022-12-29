using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Sales.Commands.DeleteSale;

public class DeleteSaleCommand : IRequest
{
    public int id { get; set; }

    public class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteSaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Sales.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Sale), request.id);
            }

            _context.Sales.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
