namespace Application.Dashboard.Queries.GetPopularCategories;

public class PopularCategoryDto
{
    public string CategoryName { get; set; } = default!;
    public int ProductCount { get; set; }
    public int Percentage { get; set; }
}
