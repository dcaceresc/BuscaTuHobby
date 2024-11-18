namespace Application.Maintainer.Menus.Queries.GetMenus;

public class MenuDto
{
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public string MenuSlug { get; set; } = default!;
    public IList<string> SubMenus { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Menu, MenuDto>()
                .ForMember(d => d.SubMenus, opt => opt.MapFrom(s => s.SubMenus.OrderBy(x => x.SubMenuOrder).Select(x => x.SubMenuName)));
        }
    }
}
