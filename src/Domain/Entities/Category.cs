namespace Domain.Entities;

public class Category : AuditableEntity
{
    private Category(string categoryName, Guid groupId)
    {
        CategoryId = new Guid();
        CategoryName = categoryName;
        GroupId = groupId;
        IsActive = true;
    }


    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; } = default!;
    public Guid GroupId { get; private set; }
    public bool IsActive { get; private set; }
    public virtual Group Group { get; private set; } = default!;
    public virtual ICollection<ProductCategory> ProductCategories { get; private set; } = default!;

    public static Category Create(string categoryName, Guid groupId)
    {
        return new Category(categoryName, groupId);
    }

    public void Update(string categoryName, Guid groupId)
    {
        CategoryName = categoryName;
        GroupId = groupId;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
