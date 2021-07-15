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

namespace Application.Grades.Queries.GetGrades
{
    public class GetGradesQuery : IRequest<IList<GradeVm>>
    {
        public class GetGradesQueryHandler : IRequestHandler<GetGradesQuery, IList<GradeVm>>
        {
            private readonly IApplicationDbContext _context;
            private readonly IMapper _mapper;

            public GetGradesQueryHandler(IApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<IList<GradeVm>> Handle(GetGradesQuery request, CancellationToken cancellationToken)
            {
                return _mapper.Map<IList<GradeVm>>(await _context.Grades.ToListAsync());
            }
        }
    }
}
