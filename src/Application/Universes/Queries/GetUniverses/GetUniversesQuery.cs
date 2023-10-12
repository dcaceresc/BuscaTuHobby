using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Universes.Queries.GetUniverses;

public class GetUniversesQuery : IRequest<IList<UniverseDto>>
{
    public class GetUniversesQueryHandler : IRequestHandler<GetUniversesQuery, IList<UniverseDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetUniversesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<UniverseDto>> Handle(GetUniversesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Universes.AsNoTracking().ProjectTo<UniverseDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

