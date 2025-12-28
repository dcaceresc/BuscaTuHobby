using Application.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;
public static class ConfigureServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRequestDispatcher, RequestDispatcher>();

        // Registro automático de todos los IRequestHandler<,> del proyecto
        services.Scan(scan => scan
            .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
