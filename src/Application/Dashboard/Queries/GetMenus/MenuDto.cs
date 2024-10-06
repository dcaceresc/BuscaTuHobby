namespace Application.Dashboard.Queries.GetMenus;

public class MenuDto
{
    public string MenuName { get; set; } = default!;
    public int MenuOrder { get; set; }
    public List<SubMenuDto> SubMenus { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Menu, MenuDto>()
                .ForMember(d => d.SubMenus, opt => opt.MapFrom(s => s.SubMenus));
        }
    }
}

public class SubMenuDto
{
    public string SubMenuName { get; set; } = default!;
    public string SubMenuSlug { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SubMenu, SubMenuDto>();
        }
    }
}