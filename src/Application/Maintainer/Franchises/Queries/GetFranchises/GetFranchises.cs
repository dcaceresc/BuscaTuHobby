namespace Application.Maintainer.Franchises.Queries.GetFranchises;
public record GetFranchises : IRequest<ApiResponse<List<FranchiseDto>>>;

public class GetFranchisesHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetFranchises, ApiResponse<List<FranchiseDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<FranchiseDto>>> Handle(GetFranchises request, CancellationToken cancellationToken)
    {
        try
        {
            var franchises = await _context.Franchises
            .AsNoTracking()
            .Select(x => new FranchiseDto
            {
                FranchiseId = x.FranchiseId,
                FranchiseName = x.FranchiseName,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

            return _responseService.Success(franchises);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<FranchiseDto>>("Error al obtener las franquicias");
        }
    }
}

