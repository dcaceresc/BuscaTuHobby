using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Configuration;

public static class InitialiserExtensions
{
    public static async Task InitialiseConfigurationAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ConfigurationInitializer>();

        await initialiser.InitialiseAsync();
    }
}

public class ConfigurationInitializer(ILogger<ConfigurationInitializer> logger, ApplicationDbContext context)
{
    private readonly ILogger<ConfigurationInitializer> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task InitialiseAsync()
    {
        try
        {
            var configurations = await _context.Configurations
                .Where(c => c.IsActive)
                .ToListAsync();


            foreach (var configuration in configurations)
            {
                SiteConfig.SiteStatus = Convert.ToBoolean(configurations.Find(x => x.ConfigurationName.Equals("SiteStatus", StringComparison.CurrentCultureIgnoreCase))?.ConfigurationValue ?? "true");
                SiteConfig.FolderImage = configurations.Find(x => x.ConfigurationName.Equals("FolderImage", StringComparison.CurrentCultureIgnoreCase))?.ConfigurationValue ?? "images";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the configuration.");
            throw;
        }
    }
}
