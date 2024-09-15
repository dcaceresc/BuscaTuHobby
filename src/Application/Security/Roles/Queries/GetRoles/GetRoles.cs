namespace Application.Security.Roles.Queries.GetRoles;
public record GetRoles : IRequest<ApiResponse<List<RoleDto>>>;

public class GetRolesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetRoles, ApiResponse<List<RoleDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<RoleDto>>> Handle(GetRoles request, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _context.Roles
                .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return _responseService.Success(roles);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<RoleDto>>("Error al obtener los permisos");
        }
    }
}
