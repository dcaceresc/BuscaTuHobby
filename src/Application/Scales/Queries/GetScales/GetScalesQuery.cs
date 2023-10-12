using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Scales.Queries.GetScales;

public class GetScalesQuery : IRequest<IList<ScaleDto>>
{
    public class GetScalesQueryHandler : IRequestHandler<GetScalesQuery, IList<ScaleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetScalesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IList<ScaleDto>> Handle(GetScalesQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<ScaleDto>>(await _context.Scales.ToListAsync());
        }
    }
}

