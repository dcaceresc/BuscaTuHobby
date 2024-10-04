namespace Application.Maintainer.Menus.Queries.GetMenuById;

public class MenuVM
{
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Menu, MenuVM>();
        }
    }
}

