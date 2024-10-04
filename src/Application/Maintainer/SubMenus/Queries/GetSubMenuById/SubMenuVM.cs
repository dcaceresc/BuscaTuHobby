namespace Application.Maintainer.SubMenus.Queries.GetSubMenuById;

public class SubMenuVM
{
    public Guid SubMenuId { get; set; }
    public string SubMenuName { get; set; } = default!;
    public Guid MenuId { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SubMenu, SubMenuVM>();
        }
    }
}
