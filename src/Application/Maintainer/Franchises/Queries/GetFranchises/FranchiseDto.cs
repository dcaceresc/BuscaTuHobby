namespace Application.Maintainer.Franchises.Queries.GetFranchises;
public class FranchiseDto
{
    public Guid FranchiseId { get; set; }
    public string FranchiseName { get; set; } = default!;
    public bool IsActive { get; set; }
}
