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
        // Resolve the WebAPI project directory (contains appsettings.json)
        var currentDir = Directory.GetCurrentDirectory();
        string basePath;

        // If CWD is already the WebAPI directory, use it directly
        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            basePath = currentDir;
        }
        else
        {
            // Fallback: navigate from Infrastructure to WebAPI
            basePath = Path.Combine(currentDir, "..", "Web", "WebAPI");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("BuscaTuHobby")
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
