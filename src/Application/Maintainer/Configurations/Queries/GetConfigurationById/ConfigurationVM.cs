namespace Application.Maintainer.Configurations.Queries.GetConfigurationById;

public class ConfigurationVM
{
    public Guid ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = default!;
    public string ConfigurationValue { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Configuration, ConfigurationVM>();
        }
    }
}