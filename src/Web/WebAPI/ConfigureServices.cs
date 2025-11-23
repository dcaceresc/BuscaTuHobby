using Application.Common.Interfaces;
using Microsoft.OpenApi.Models;
using WebAPI.Services;

namespace WebAPI;

public static class ConfigureServices
{
    public static IServiceCollection AddWebAPI(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddHttpContextAccessor();
        services.AddCarter();
        services.AddSwaggerGen(c =>
        {

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BuscaTuHobby API",
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
              Array.Empty<string>()
            }
          });
        });



        services.AddSingleton<IAuthenticationService, AuthenticationService>();


        services.AddAuthorizationBuilder()
           .AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"))
           .AddPolicy("Administrator", policy => policy.RequireRole("SuperAdmin", "Administrator"));

        return services;
    }
}
