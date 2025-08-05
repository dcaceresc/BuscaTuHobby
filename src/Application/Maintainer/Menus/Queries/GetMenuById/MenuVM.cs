namespace Application.Maintainer.Menus.Queries.GetMenuById;

public class MenuVM
{
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public int MenuOrder { get; set; }
    public IList<SubMenuDto> SubMenus { get; set; } = default!;
}

public class SubMenuDto
{
    public Guid SubMenuId { get; set; }
    public string SubMenuName { get; set; } = default!;
    public int SubMenuOrder { get; set; }
    public bool IsActive { get; set; }
    
}

