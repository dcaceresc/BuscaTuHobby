using Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Photos.Queries.GetPhotos;

public class GetPhotosQuery : IRequest<IList<PhotoVm>>
{
    public class GetPhotosQueryHandler : IRequestHandler<GetPhotosQuery, IList<PhotoVm>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetPhotosQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<PhotoVm>> Handle(GetPhotosQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<IList<PhotoVm>>(await _context.Photos.ToListAsync());
        }
    }
}



