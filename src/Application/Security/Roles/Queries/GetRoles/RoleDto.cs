namespace Application.Security.Roles.Queries.GetRoles;

public class RoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = default!;
    public bool IsActive { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Role, RoleDto>();
        }
    }
}