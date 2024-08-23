namespace Application.Maintainer.Franchises.Queries.GetFranchiseById;
public record GetFranchiseById(Guid FranchiseId) : IRequest<ApiResponse<FranchiseVM>>;

public class GetFranchiseByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetFranchiseById, ApiResponse<FranchiseVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<FranchiseVM>> Handle(GetFranchiseById request, CancellationToken cancellationToken)
    {
        try
        {
            var franchises = await _context.Franchises
            .Where(x => x.FranchiseId == request.FranchiseId)
            .AsNoTracking()
            .ProjectTo<FranchiseVM>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

            Guard.Against.NotFound(franchises, $"No existe franquicia con la Id {request.FranchiseId}");

            return _responseService.Success(franchises);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<FranchiseVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<FranchiseVM>("Error al obtener la franquicia");
        }
    }
}