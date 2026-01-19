namespace Domain.Entities;

public class Category : AuditableEntity
{
    private Category(string categoryName, string categoryIcon, int categoryOrder, string categorySlug)
    {
        CategoryId = Guid.NewGuid();
        CategoryName = categoryName;
        CategoryIcon = categoryIcon;
        CategoryOrder = categoryOrder;
        CategorySlug = categorySlug;
        IsActive = true;
    }


    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; } = default!;
    public string CategoryIcon {get; private set; } = default!;
    public int CategoryOrder { get; set; }
    public string CategorySlug { get; set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<ProductCategory> ProductCategories { get; private set; } = default!;
    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();


    public static Category Create(string categoryName, string categoryIcon, int categoryOrder, string categorySlug)
    {
        return new Category(categoryName, categoryIcon,  categoryOrder, categorySlug);
    }

    public void Update(string categoryName, string categoryIcon, int categoryOrder, string categorySlug)
    {
        CategoryName = categoryName;
        CategoryIcon = categoryIcon;
        CategoryOrder = categoryOrder;
        CategorySlug = categorySlug;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
