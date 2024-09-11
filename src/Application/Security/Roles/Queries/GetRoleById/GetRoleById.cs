﻿namespace Application.Security.Roles.Queries.GetRoleById;
public record GetRoleById(Guid RoleId) : IRequest<ApiResponse<RoleVM>>;

public class GetRoleByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetRoleById, ApiResponse<RoleVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<RoleVM>> Handle(GetRoleById request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles
                .ProjectTo<RoleVM>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.RoleId == request.RoleId, cancellationToken);

            Guard.Against.NotFound(role, $"No existe permiso con la Id {request.RoleId}");

            return _responseService.Success(role);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<RoleVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<RoleVM>("Error al obtener el permiso");
        }
    }
}
