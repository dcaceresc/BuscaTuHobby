namespace Application.Maintainer.Communes.Commands.CreateCommune;
public record CreateCommune(string CommuneName, Guid RegionId) : IRequest<ApiResponse>;

public class CreateCommuneHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateCommune, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateCommune request, CancellationToken cancellationToken)
    {
        try
        {
            var commune = Commune.Create(request.CommuneName, request.RegionId);

            _context.Communes.Add(commune);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La cuidad ha sido creada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un error al crear la ciudad");
        }
    }
}
