namespace Application.Maintainer.SubMenus.Queries.GetSubMenus;
public record GetSubMenus : IRequest<ApiResponse<List<SubMenuDto>>>;

public class GetSubMenusHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetSubMenus, ApiResponse<List<SubMenuDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<SubMenuDto>>> Handle(GetSubMenus request, CancellationToken cancellationToken)
    {
        try
        {
            var subMenus = await _context.SubMenus
            .Include(x => x.Menu)
            .AsNoTracking()
            .ProjectTo<SubMenuDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(subMenus);

        }
        catch (Exception)
        {
            return _responseService.Fail<List<SubMenuDto>>("Error al obtener las sub menus");
        }
    }
}


