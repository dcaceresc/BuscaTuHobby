namespace Application.Maintainer.Groups.Commands.CreateGroup;
public record CreateGroup(string GroupName) : IRequest<ApiResponse>;

public class CreateGroupHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateGroup, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateGroup request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Group.Create(request.GroupName);

            _context.Groups.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Grupo creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear el grupo");
        }
    }
}
