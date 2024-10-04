namespace Application.Maintainer.SubMenus.Queries.GetSubMenus;

public class SubMenuDto
{
    public Guid SubMenuId { get; set; }
    public string SubMenuName { get; set; } = default!;
    public string MenuName { get; set; } = default!;
    public bool IsActive { get; set; }



    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SubMenu, SubMenuDto>().
                ForMember(d => d.MenuName, opt => opt.MapFrom(x => x.Menu.MenuName));
        }
    }
}