using Application.Common.Ingestion;
using Infrastructure.Configuration;
using Infrastructure.Data.Interceptors;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure;

public static class ConfigureServices
{
    private const string RobotsClient = "store-robots";

    // El transporte no sigue redirecciones: una redirección a otro host saltaría la comprobación de robots y
    // el limitador del dominio solicitado.
    private static HttpClientHandler StoreTransport() => new()
    {
        AllowAutoRedirect = false,
        UseCookies = false
    };
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(configuration.GetConnectionString("BuscaTuHobby"));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped<ConfigurationInitializer>();

        services.AddScoped<IApiResponseService, ApiResponseService>();
        services.AddScoped<IUtilityService, UtilityService>();
        services.AddScoped<IEmailService, EmailService>();

        // Política de cortesía compartida por todas las tiendas: una petición a la vez por dominio, sin importar
        // cuántos conectores o instancias de cliente estén activos en el proceso.
        services.AddSingleton<DomainRequestLimiter>();
        services.AddHttpClient(RobotsClient, client => client.Timeout = TimeSpan.FromSeconds(15))
            .ConfigurePrimaryHttpMessageHandler(StoreTransport);
        services.AddSingleton(provider => new RobotsTxtCache(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(RobotsClient),
            provider.GetRequiredService<DomainRequestLimiter>()));

        services.AddScoped<IStoreContextProvider, StoreContextProvider>();
        services.AddHttpClient<IStoreConnector, ShopifyConnector>(client =>
            client.Timeout = TimeSpan.FromSeconds(15))
            .ConfigurePrimaryHttpMessageHandler(StoreTransport);
        // Las tiendas que publican su ficha con JSON-LD usan un conector más, con el mismo limitador y el mismo portal
        // de robots. La tienda elige por la clave de su fila, así que una tienda nueva sigue siendo una fila.
        services.AddHttpClient<IStoreConnector, JsonLdConnector>(client =>
            client.Timeout = TimeSpan.FromSeconds(15))
            .ConfigurePrimaryHttpMessageHandler(StoreTransport);

        // El descubrimiento usa el mismo limitador y el mismo portal de robots que la lectura de fichas: así una
        // ejecución no duplica la política de cortesía ni puede saltarse robots.txt por ir por otro cliente.
        services.AddHttpClient<ICatalogDiscovery, SitemapCatalogDiscovery>(client =>
            client.Timeout = TimeSpan.FromSeconds(15))
            .ConfigurePrimaryHttpMessageHandler(StoreTransport);
        services.AddScoped<IImportRunService, ImportRunService>();
        // Ingesta programada: apagada por defecto. Ingestion:Enabled la enciende e Ingestion:IntervalHours fija cada
        // cuánto corre; appsettings.Development.json la enciende porque un catálogo de demostración no se refresca solo.
        // En producción nadie la enciende sin decirlo en la configuración de ese entorno.
        services.AddSingleton(IngestionSettings.Parse(
            configuration[IngestionSettings.EnabledKey], configuration[IngestionSettings.IntervalHoursKey]));
        // El disparo manual y la vuelta programada comparten ejecutor: una sola puerta para descubrir y procesar.
        services.AddScoped<IIngestionRunner, IngestionRunner>();
        services.AddHostedService<IngestionWorker>();
        // La pasada que convierte lo encolado en ofertas recibe todos los conectores registrados: la tienda elige por
        // la clave de su fila y el limitador y robots.txt siguen dentro del conector.
        services.AddScoped<IImportRunProcessor>(provider => new ImportRunProcessor(
            provider.GetRequiredService<IApplicationDbContext>(),
            provider.GetServices<IStoreConnector>()));
        // El cargador recibe todos los conectores registrados y elige por la clave de la fila: añadir una tienda
        // Shopify no toca esta composición, solo añade una fila.
        services.AddScoped<IStoreProductLoader>(provider => new StoreProductLoader(
            provider.GetRequiredService<IStoreContextProvider>(),
            provider.GetServices<IStoreConnector>()));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                        };
                    });

        return services;
    }
}

