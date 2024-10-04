namespace Application.Maintainer.Menus.Queries.GetMenuById;

public record GetMenuById(Guid MenuId) : IRequest<ApiResponse<MenuVM>>;

public class GetCategoryByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetMenuById, ApiResponse<MenuVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<MenuVM>> Handle(GetMenuById request, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await _context.Menus.
            ProjectTo<MenuVM>(_mapper.ConfigurationProvider).
            FirstOrDefaultAsync(x => x.MenuId == request.MenuId, cancellationToken);

            Guard.Against.NotFound(menu, $"No existe menu con la Id {request.MenuId}");

            return _responseService.Success(menu);
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail<MenuVM>(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<MenuVM>("No se pudo obtener el menu");
        }
    }
}

