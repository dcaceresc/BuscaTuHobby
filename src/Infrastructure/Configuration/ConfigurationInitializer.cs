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

        // La raíz de contenido es la única base válida para una carpeta relativa: el directorio de trabajo del
        // proceso puede ser otro (systemd con WorkingDirectory=/, IIS en proceso) y el escritor de imágenes no
        // pasa por el pipeline HTTP, así que aquí se resuelve una sola vez y ambos lados leen el mismo valor.
        await initialiser.InitialiseAsync(app.Environment.ContentRootPath);
    }
}

public class ConfigurationInitializer(ILogger<ConfigurationInitializer> logger, ApplicationDbContext context)
{
    private readonly ILogger<ConfigurationInitializer> _logger = logger;
    private readonly ApplicationDbContext _context = context;

    public async Task InitialiseAsync(string contentRootPath)
    {
        try
        {
            var configurations = await _context.Configurations
                .Where(c => c.IsActive)
                .ToListAsync();


            foreach (var configuration in configurations)
            {
                SiteConfig.SiteStatus = Convert.ToBoolean(configurations.Find(x => x.ConfigurationName.Equals("SiteStatus", StringComparison.CurrentCultureIgnoreCase))?.ConfigurationValue ?? "true");
                SiteConfig.FolderImage = SiteConfig.ResolveFolderImage(
                    configurations.Find(x => x.ConfigurationName.Equals("FolderImage", StringComparison.CurrentCultureIgnoreCase))?.ConfigurationValue,
                    contentRootPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the configuration.");
            throw;
        }
    }
}
