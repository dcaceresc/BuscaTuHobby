using Domain.Entities;

namespace Application.Maintainer.Groups.Queries.GetGroupById
{
    public class GroupVM
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
    }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Group, GroupVM>();
        }
    }
}