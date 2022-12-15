using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Series.Queries.GetSeries;

public class GetSeriesQuery : IRequest<IList<SerieVm>>
{
    public class GetSeriesQueryHandler : IRequestHandler<GetSeriesQuery, IList<SerieVm>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSeriesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<SerieVm>> Handle(GetSeriesQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<SerieVm>>(await _context.Series.ToListAsync());
        }
    }
}

