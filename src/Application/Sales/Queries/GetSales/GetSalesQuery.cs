
using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Sales.Queries.GetSales;

public class GetSalesQuery : IRequest<IList<SaleVm>>
{
    public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, IList<SaleVm>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSalesQueryHandler(IApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<SaleVm>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<SaleVm>>(await _context.Photos.ToListAsync());
        }
    }
}