namespace Application.Maintainer.Series.Commands.CreateSerie;

public record CreateSerie(string SerieName, Guid FranchiseId) : IRequest<ApiResponse>;

public class CreateSerieHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateSerie, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateSerie request, CancellationToken cancellationToken)
    {
        try
        {
            var serie = Serie.Create(request.SerieName, request.FranchiseId);

            _context.Series.Add(serie);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Serie creada correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al crear la serie");
        }


    }
}