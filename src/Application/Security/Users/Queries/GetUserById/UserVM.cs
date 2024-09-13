namespace Application.Security.Users.Queries.GetUserById;

public class UserVM
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public IList<Guid> RoleIds { get; set; } = default!;



    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<User, UserVM>()
                .ForMember(d => d.RoleIds, opt => opt.MapFrom(s => s.UserRoles.Select(x => x.RoleId).ToList()));
        }
    }
}