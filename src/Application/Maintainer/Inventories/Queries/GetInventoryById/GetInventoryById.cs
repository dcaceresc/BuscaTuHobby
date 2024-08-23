namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public record GetInventoryById(Guid InventoryId) : IRequest<ApiResponse<InventoryVM>>;

public class GetInventoryByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetInventoryById, ApiResponse<InventoryVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<InventoryVM>> Handle(GetInventoryById request, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await _context.Inventories
               .AsNoTracking()
               .ProjectTo<InventoryVM>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync(x => x.InventoryId == request.InventoryId, cancellationToken);

            Guard.Against.NotFound(inventory, $"No existe inventario con Id {request.InventoryId}");

            return _responseService.Success(inventory);
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail<InventoryVM>(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<InventoryVM>("No se pudo obtener el inventario");
        }
    }
}
