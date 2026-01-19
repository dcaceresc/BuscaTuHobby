namespace Application.Maintainer.Stores.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid StoreId) : IRequest<ApiResponse<StoreVM>>;

public class GetStoreByIdQueryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetStoreByIdQuery, ApiResponse<StoreVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<StoreVM>> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var store = await _context.Stores
              .Include(x => x.StoreAddresses)
              .AsNoTracking()
              .Select(x => new StoreVM
              {
                  StoreId = x.StoreId,
                  StoreName = x.StoreName,
                  StoreWebSite = x.StoreWebSite,
                  StoreIcon = x.StoreIcon,
                  StoreAddress = x.StoreAddresses
                      .Select(sa => new StoreAddressDto()
                      {
                          StoreAddressId = sa.StoreAddressId,
                          CommuneId = sa.CommuneId,
                          RegionId = sa.Commune.RegionId,
                          ZipCode = sa.ZipCode,
                          Street = sa.Street,
                      }).ToList()
              })
              .FirstOrDefaultAsync(x => x.StoreId == request.StoreId, cancellationToken);

            Guard.Against.NotFound(store, $"No existe tienda con la Id {request.StoreId}");

            return _responseService.Success(store);

        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<StoreVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<StoreVM>("Ocurrió un error al obtener la tienda");
        }
    }
}
