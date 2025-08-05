namespace Application.Maintainer.Configurations.Queries.GetConfigurations;

public class ConfigurationDto
{
    public Guid ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = default!;
    public string ConfigurationValue { get; set; } = default!;
    public bool IsActive { get; set; }
}