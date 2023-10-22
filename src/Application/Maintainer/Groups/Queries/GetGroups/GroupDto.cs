using Domain.Entities;

namespace Application.Maintainer.Groups.Queries.GetGroups;

public class GroupDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Group, GroupDto>();
        }
    }
}

