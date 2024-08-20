namespace Application.Maintainer.Groups.Queries.GetGroups;

public class GroupDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Group, GroupDto>();
        }
    }
}

