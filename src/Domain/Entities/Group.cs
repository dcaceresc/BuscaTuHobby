namespace Domain.Entities;

public class Group : AuditableEntity
{
    private Group(string groupName)
    {
        GroupId = Guid.NewGuid();
        GroupName = groupName;
        IsActive = true;
    }


    public Guid GroupId { get; private set; }
    public string GroupName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<Category> Categories { get; private set; } = default!;

    public static Group Create(string groupName)
    {
        return new Group(groupName);
    }

    public void Update(string groupName)
    {
        GroupName = groupName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
