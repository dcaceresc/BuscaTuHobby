namespace Domain.Entities;

public class Menu : AuditableEntity
{
    private Menu(string menuName)
    {
        MenuId = Guid.NewGuid();
        MenuName = menuName;
        IsActive = true;
    }


    public Guid MenuId { get; private set; }
    public string MenuName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<SubMenu> SubMenus { get; private set; } = default!;

    public static Menu Create(string menuName)
    {
        return new Menu(menuName);
    }

    public void Update(string groupName)
    {
        MenuName = groupName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
