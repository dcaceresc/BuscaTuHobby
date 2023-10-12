using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Series.Queries.GetSeries;

public class GetSeriesQuery : IRequest<IList<SerieDto>>
{
    public class GetSeriesQueryHandler : IRequestHandler<GetSeriesQuery, IList<SerieDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSeriesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<SerieDto>> Handle(GetSeriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Series.Include(x => x.Universe).AsNoTracking().ProjectTo<SerieDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

