using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Resuelve el directorio del proyecto WebAPI (contiene appsettings.json)
        var currentDir = Directory.GetCurrentDirectory();
        string basePath;

        // Si el directorio de trabajo ya es WebAPI, se usa directamente
        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            basePath = currentDir;
        }
        else
        {
            // Alternativa: navegar de Infrastructure a WebAPI
            basePath = Path.Combine(currentDir, "..", "Web", "WebAPI");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            // Usa el mismo almacén que WebAPI; las variables de entorno conservan la precedencia.
            .AddUserSecrets("BuscaTuHobby-WebAPI-8d7a90e6-4523-44a9-a234-114a1e6a3c91")
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("BuscaTuHobby")
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
