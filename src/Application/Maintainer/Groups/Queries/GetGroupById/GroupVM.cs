using Domain.Entities;

namespace Application.Maintainer.Groups.Queries.GetGroupById;

public class GroupVM
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Group, GroupVM>();
        }
    }
}

