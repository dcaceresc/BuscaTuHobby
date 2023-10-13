using Application.Common.Interfaces;
using Microsoft.OpenApi.Models;
using WebUI.Services;

namespace Web;

public static class ConfigureServices
{
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BuscaTuGunpla API",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
           {
             new OpenApiSecurityScheme
             {
               Reference = new OpenApiReference
               {
                 Type = ReferenceType.SecurityScheme,
                 Id = "Bearer"
               }
              },
              new string[] { }
            }
          });
        });

        services.AddControllersWithViews();
        services.AddRazorPages();



        return services;
    }
}