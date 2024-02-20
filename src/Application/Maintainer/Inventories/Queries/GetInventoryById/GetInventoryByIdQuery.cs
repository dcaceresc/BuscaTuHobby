namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public record GetInventoryByIdQuery(Guid InventoryId) : IRequest<InventoryVM>;

public class GetInventoryIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetInventoryByIdQuery, InventoryVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<InventoryVM> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories
               .AsNoTracking()
               .ProjectTo<InventoryVM>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync(x => x.InventoryId == request.InventoryId, cancellationToken);

        Guard.Against.NotFound(request.InventoryId, inventory);

        return inventory;
    }
}
