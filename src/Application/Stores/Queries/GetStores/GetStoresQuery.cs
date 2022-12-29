
using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Queries.GetStores;

public class GetStoresQuery : IRequest<IList<StoreVm>>
{
    public class GetStoresQueryHandler : IRequestHandler<GetStoresQuery, IList<StoreVm>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;


        public GetStoresQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<StoreVm>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<StoreVm>>(await _context.Stores.ToListAsync());
        }
    }
}