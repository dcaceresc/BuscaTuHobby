namespace Domain.Entities;

public class SubMenu : AuditableEntity
{
    public SubMenu()
    {
        
    }

    private SubMenu(string subMenuName, Guid menuId, int subMenuOrden)
    {
        SubMenuId = new Guid();
        SubMenuName = subMenuName;
        SubMenuOrder = subMenuOrden;
        SubMenuSlug = subMenuName.ToLower().Replace(" ", "-");
        MenuId = menuId;
        IsActive = true;
    }


    public Guid SubMenuId { get; private set; }
    public string SubMenuName { get; private set; } = default!;
    public int SubMenuOrder { get; private set; }
    public string SubMenuSlug { get; private set; } = default!;
    public Guid MenuId { get; private set; }
    public bool IsActive { get; private set; }
    public virtual Menu Menu { get; private set; } = default!;

    public static SubMenu Create(string subMenuName, Guid menuId, int subMenuOrden)
    {
        return new SubMenu(subMenuName, menuId,subMenuOrden);
    }

    public void Update(string subMenuName, Guid menuId, int subMenuOrden)
    {
        SubMenuName = subMenuName;
        MenuId = menuId;
        SubMenuOrder = subMenuOrden;
        SubMenuSlug = subMenuName.ToLower().Replace(" ", "-");
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
