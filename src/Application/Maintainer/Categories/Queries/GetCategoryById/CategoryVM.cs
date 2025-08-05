namespace Application.Maintainer.Categories.Queries.GetCategoryById;

public class CategoryVM
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
}
