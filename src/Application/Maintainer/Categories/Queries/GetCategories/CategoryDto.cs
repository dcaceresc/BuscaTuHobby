namespace Application.Maintainer.Categories.Queries.GetCategories;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string CategoryIcon { get; set; } = default!;
    public int CategoryOrder { get; set; }
    public string CategorySlug { get; set; } = default!;
    public bool IsActive { get; set; }
}

