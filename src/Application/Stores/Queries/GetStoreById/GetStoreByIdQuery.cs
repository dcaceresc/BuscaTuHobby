namespace Application.Stores.Queries.GetStoreById;

public class GetStoreByIdQuery : IRequest<StoreVM>
{
    public int id { get; set; }
}

public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, StoreVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStoreByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StoreVM> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Stores.Where(x => x.id == request.id).AsNoTracking().ProjectTo<StoreVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}
