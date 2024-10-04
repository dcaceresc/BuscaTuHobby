namespace Domain.Entities;

public class SubMenu : AuditableEntity
{
    private SubMenu(string subMenuName, Guid menuId)
    {
        SubMenuId = new Guid();
        SubMenuName = subMenuName;
        MenuId = menuId;
        IsActive = true;
    }


    public Guid SubMenuId { get; private set; }
    public string SubMenuName { get; private set; } = default!;
    public Guid MenuId { get; private set; }
    public bool IsActive { get; private set; }
    public virtual Menu Menu { get; private set; } = default!;

    public static SubMenu Create(string subMenuName, Guid menuId)
    {
        return new SubMenu(subMenuName, menuId);
    }

    public void Update(string subMenuName, Guid menuId)
    {
        SubMenuName = subMenuName;
        MenuId = menuId;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
