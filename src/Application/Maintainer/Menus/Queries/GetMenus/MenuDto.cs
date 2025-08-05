namespace Application.Maintainer.Menus.Queries.GetMenus;

public class MenuDto
{
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public string MenuSlug { get; set; } = default!;
    public IList<string> SubMenus { get; set; } = default!;
    public bool IsActive { get; set; }
}
