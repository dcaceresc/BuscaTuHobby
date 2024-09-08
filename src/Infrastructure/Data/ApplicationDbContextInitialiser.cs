using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}


public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, IIdentityService identityService)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly ApplicationDbContext _context = context;
    private readonly IIdentityService _identityService = identityService;

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        await SeedRoleAsync("SuperAdmin");
        await SeedRoleAsync("Administrator");
        await SeedRoleAsync("User");

        await SeedUserAsync("admin@localhost", "admin123", "SuperAdmin");

        await _context.SaveChangesAsync();
    }

    private async Task SeedRoleAsync(string roleName)
    {
        if (!_context.Roles.Any(x => x.RoleName == roleName))
        {
            var role = Role.Create(roleName);
            await _context.Roles.AddAsync(role);
        }
    }

    private async Task SeedUserAsync(string email, string password, string roleName)
    {
        if (!_context.Users.Any(x => x.Email == email))
        {
            var user = User.Create(email, _identityService.HashPassword(password));

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            var role = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == roleName);

            if (role != null)
            {
                var userRole = user.AssignRole(role.RoleId);
                if (!_context.UserRoles.Any(x => x.UserId == user.UserId && x.RoleId == role.RoleId))
                {
                    await _context.UserRoles.AddAsync(userRole);
                }
            }
        }
    }
}
