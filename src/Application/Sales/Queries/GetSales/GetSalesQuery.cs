
using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sales.Queries.GetSales;

public class GetSalesQuery : IRequest<IList<SaleDto>>
{
    public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, IList<SaleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSalesQueryHandler(IApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<SaleDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<SaleDto>>(await _context.Photos.ToListAsync());
        }
    }
}