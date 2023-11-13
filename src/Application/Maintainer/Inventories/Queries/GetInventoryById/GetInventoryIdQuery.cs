namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public class GetInventoryIdQuery : IRequest<InventoryVM>
{
    public int id { get; set; }
}

public class GetInventoryIdQueryHandler : IRequestHandler<GetInventoryIdQuery, InventoryVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<InventoryVM> Handle(GetInventoryIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Inventories.Where(x => x.id == request.id).AsNoTracking().ProjectTo<InventoryVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}
