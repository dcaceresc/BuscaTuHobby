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

namespace Application.Gunplas.Queries.GetGunplas
{
    public class GetGunplasQuery : IRequest<IList<GunplaVm>>
    {
        public class GetGunplasQueryHandler : IRequestHandler<GetGunplasQuery, IList<GunplaVm>>
        {
            private readonly IApplicationDbContext _context;
            private readonly IMapper _mapper;

            public GetGunplasQueryHandler(IApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IList<GunplaVm>> Handle(GetGunplasQuery request, CancellationToken cancellationToken)
            {

                return _mapper.Map<IList<GunplaVm>>(await _context.Gunplas.ToListAsync());

            }
        }
    }
}
