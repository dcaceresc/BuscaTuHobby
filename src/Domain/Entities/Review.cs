namespace Domain.Entities;

public class Review
{
    public int id { get; set; }
    public int storeId { get; set; }
    public string userId { get; set; } = default!;
    public int ranking { get; set; }
    public string message { get; set; } = default!;

    public Store Store { get; set; } = default!;
}