using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Sales.Commands.UpdateSale;

public class UpdateSaleCommand : IRequest
{
    public int id { get; set; }
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }

    public class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Sales.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Sale), request.id);
            }

            entity.gunplaId = request.gunplaId;
            entity.storeId = request.storeId;
            entity.price = request.price;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}