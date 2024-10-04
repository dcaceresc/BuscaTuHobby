namespace Domain.Entities;

public class Category : AuditableEntity
{
    private Category(string categoryName)
    {
        CategoryId = Guid.NewGuid();
        CategoryName = categoryName;
        IsActive = true;
    }


    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<ProductCategory> ProductCategories { get; private set; } = default!;


    public static Category Create(string categoryName)
    {
        return new Category(categoryName);
    }

    public void Update(string categoryName)
    {
        CategoryName = categoryName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
