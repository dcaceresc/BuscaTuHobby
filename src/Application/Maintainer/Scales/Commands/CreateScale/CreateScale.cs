namespace Application.Maintainer.Scales.Commands.CreateScale;

public record CreateScale(string ScaleName) : IRequest<ApiResponse>;

public class CreateScaleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateScale, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateScale request, CancellationToken cancellationToken)
    {
        try
        {
            var scale = Scale.Create(request.ScaleName);

            _context.Scales.Add(scale);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Escala creada correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al crear la escala");
        }
    }
}