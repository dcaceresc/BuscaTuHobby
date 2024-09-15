namespace Application.Security.Users.Queries.GetUsers;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public string LockoutEnd { get; set; } = default!;
    public bool LockoutEnabled { get; set; }
    public IList<string> RoleNames { get; set; } = default!;
    public bool IsActive { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.LockoutEnd, opt => opt.MapFrom(s => s.LockoutEnd.HasValue ? s.LockoutEnd.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty))
                .ForMember(d => d.RoleNames, opt => opt.MapFrom(s => s.UserRoles.Select(x => x.Role.RoleName).ToList()));
        }
    }
}