using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Universes.Queries.GetUniverses;

public class GetUniversesQuery : IRequest<IList<UniverseVm>>
{
    public class GetUniversesQueryHandler : IRequestHandler<GetUniversesQuery, IList<UniverseVm>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetUniversesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<UniverseVm>> Handle(GetUniversesQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<UniverseVm>>(await _context.Universes.ToListAsync());
        }
    }
}

