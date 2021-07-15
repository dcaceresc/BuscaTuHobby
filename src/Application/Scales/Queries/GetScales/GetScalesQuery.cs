using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Scales.Queries.GetScales
{
    public class GetScalesQuery : IRequest<IList<ScaleVm>>
    {
        public class GetScalesQueryHandler : IRequestHandler<GetScalesQuery, IList<ScaleVm>>
        {
            private readonly IApplicationDbContext _context;
            private readonly IMapper _mapper;

            public GetScalesQueryHandler(IApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }
            public async Task<IList<ScaleVm>> Handle(GetScalesQuery request, CancellationToken cancellationToken)
            {
                return _mapper.Map<IList<ScaleVm>>(await _context.Scales.ToListAsync());
            }
        }
    }
}
