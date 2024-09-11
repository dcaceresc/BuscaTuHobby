namespace Application.Security.Roles.Queries.GetRoleById;

public class RoleVM
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Role, RoleVM>();
        }
    }
}