namespace Application.Maintainer.SubMenus.Queries.GetSubMenuById;

public record GetSubMenuById(Guid SubMenuId) : IRequest<ApiResponse<SubMenuVM>>;

public class GetSubMenuByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetSubMenuById, ApiResponse<SubMenuVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<SubMenuVM>> Handle(GetSubMenuById request, CancellationToken cancellationToken)
    {
        try
        {
            var subMenu = await _context.SubMenus
            .Where(x => x.SubMenuId == request.SubMenuId)
            .AsNoTracking()
            .ProjectTo<SubMenuVM>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(subMenu, $"No existe sub menus con la Id {request.SubMenuId}");

            return _responseService.Success(subMenu);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<SubMenuVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<SubMenuVM>("Error al obtener la sub menus");
        }

    }
}
