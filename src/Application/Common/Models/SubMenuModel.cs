namespace Application.Common.Models;

public record CreateSubMenu
{
    public string SubMenuName { get; init; } = default!;
    public int SubMenuOrder { get; init; }
}

public record UpdateSubMenu
{
    public Guid SubMenuId { get; set; }
    public string SubMenuName { get; set; } = default!;
    public int SubMenuOrder { get; set; }
}