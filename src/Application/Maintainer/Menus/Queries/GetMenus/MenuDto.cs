namespace Application.Maintainer.Menus.Queries.GetMenus;

public class MenuDto
{
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Menu, MenuDto>();
        }
    }
}

