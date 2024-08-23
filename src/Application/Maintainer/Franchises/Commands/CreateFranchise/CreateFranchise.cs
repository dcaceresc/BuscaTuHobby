namespace Application.Maintainer.Franchises.Commands.CreateFranchise;
public record CreateFranchise(string FranchiseName) : IRequest<ApiResponse>;

public class CreateFranchiseHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateFranchise, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateFranchise request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Franchise.Create(request.FranchiseName);

            _context.Franchises.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Franquicia creada exitosamente");

        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear la franquicia");
        }
    }
}
