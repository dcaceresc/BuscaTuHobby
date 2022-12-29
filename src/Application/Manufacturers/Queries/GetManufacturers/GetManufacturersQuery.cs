using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Manufacturers.Queries.GetManufacturers;

public class GetManufacturersQuery : IRequest<IList<ManufacturerVm>>
{
    public class GetManufacturersQueryHandler : IRequestHandler<GetManufacturersQuery, IList<ManufacturerVm>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetManufacturersQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IList<ManufacturerVm>> Handle(GetManufacturersQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<ManufacturerVm>>(await _context.Manufacturers.ToListAsync());
        }
    }
}