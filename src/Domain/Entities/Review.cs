namespace Domain.Entities;

public class Review : AuditableEntity
{
    public Guid ReviewId { get; set; }
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public int ReviewRanking { get; set; }
    public string ReviewMessage { get; set; } = default!;

    public Store Store { get; set; } = default!;
    public User User { get; set; } = default!;
}