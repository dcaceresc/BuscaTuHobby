namespace Domain.Entities;

public class SubCategory
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int categoryId { get; set; }
    public bool active { get; set; }
    public virtual Category Category { get; set; } = default!;
    public virtual ICollection<SubCategoryProduct> SubCategoryProducts { get; set; } = default!;

}
