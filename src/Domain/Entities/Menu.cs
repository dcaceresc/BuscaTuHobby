namespace Domain.Entities;

public class Menu : AuditableEntity
{
    public Menu()
    {
        
    }

    private Menu(string menuName, int menuOrden)
    {
        MenuId = Guid.NewGuid();
        MenuName = menuName;
        MenuOrder = menuOrden;
        IsActive = true;
    }


    public Guid MenuId { get; private set; }
    public string MenuName { get; private set; } = default!;
    public int MenuOrder { get; private set; }
    public bool IsActive { get; private set; }

    public virtual ICollection<SubMenu> SubMenus { get; private set; } = default!;

    public static Menu Create(string menuName, int menuOrden)
    {
        return new Menu(menuName,menuOrden);
    }

    public void Update(string menuName, int menuOrden)
    {
        MenuName = menuName;
        MenuOrder = menuOrden;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
