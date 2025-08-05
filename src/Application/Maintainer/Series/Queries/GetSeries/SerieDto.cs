namespace Application.Maintainer.Series.Queries.GetSeries;

public class SerieDto
{
    public Guid SerieId { get; set; }
    public string SerieName { get; set; } = default!;
    public string FranchiseName { get; set; } = default!;
    public bool IsActive { get; set; }
    
}

