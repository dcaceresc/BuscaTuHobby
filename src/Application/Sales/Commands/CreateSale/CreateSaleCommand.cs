using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Sales.Commands.CreateSale;

public class CreateSaleCommand : IRequest<int>
{
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }

    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateSaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var entity = new Sale
            {
                gunplaId = request.gunplaId,
                storeId = request.storeId,
                price = request.price
            };

            _context.Sales.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }

}
