namespace Application.Dashboard.Queries.GetMenus;

public class MenuDto
{
    public string MenuName { get; set; } = default!;
    public int MenuOrder { get; set; }
    public string MenuSlug { get; set; } = default!;
    public List<SubMenuDto> SubMenus { get; set; } = default!;
}

public class SubMenuDto
{
    public string SubMenuName { get; set; } = default!;
    public int SubMenuOrder { get; set; }
    public string SubMenuSlug { get; set; } = default!;
}