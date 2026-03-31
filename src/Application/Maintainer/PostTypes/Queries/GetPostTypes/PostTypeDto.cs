namespace Application.Maintainer.PostTypes.Queries.GetPostTypes;

public class PostTypeDto
{
    public Guid PostTypeId { get; set; }
    public string PostTypeName { get; set; } = default!;
    public bool IsActive { get; set; }
}
