namespace Application.Maintainer.PostTypes.Queries.GetPostTypeById;

public class PostTypeVM
{
    public Guid PostTypeId { get; set; }
    public string PostTypeName { get; set; } = default!;
}
