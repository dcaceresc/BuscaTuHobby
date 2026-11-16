using Application;
using Domain.Common;
using Infrastructure;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Microsoft.Extensions.FileProviders;
using WebAPI;


var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor.
// Más información sobre cómo configurar Swagger/OpenAPI en https://aka.ms/aspnetcore/swashbuckle


builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebAPI();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configurar el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

await app.InitialiseConfigurationAsync();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

// Las imágenes subidas de los productos viven fuera de la raíz web, en la carpeta indicada por SiteConfig.FolderImage.
// El nombre solo se conoce después de leer la tabla de configuración, así que el mapeo se registra aquí: después de
// InitialiseConfigurationAsync y después de la redirección HTTPS, y antes de MapCarter y MapFallbackToFile para que
// una solicitud a /images/{id}.jpg la responda este middleware en lugar del index.html del SPA.
var imagesFolder = PrepareImagesFolder(app.Environment, app.Logger);
if (imagesFolder is not null)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesFolder),
        RequestPath = "/images"
    });
}

app.MapCarter();

app.MapFallbackToFile("/index.html");

app.Run();

// Una carpeta relativa (la predeterminada, "images") se resuelve contra la raíz de contenido; una absoluta se usa
// tal cual. La carpeta se crea si falta. Si no se puede crear, la aplicación igual arranca, sin /images: una
// carpeta de subidas rota no debe tumbar el sitio entero.
static string? PrepareImagesFolder(IWebHostEnvironment environment, ILogger logger)
{
    var configured = string.IsNullOrWhiteSpace(SiteConfig.FolderImage) ? "images" : SiteConfig.FolderImage;
    var folder = Path.IsPathRooted(configured) ? configured : Path.Combine(environment.ContentRootPath, configured);
    try
    {
        Directory.CreateDirectory(folder);
        return folder;
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
    {
        logger.LogWarning(exception, "No se pudo preparar la carpeta de imágenes {Folder}; /images no se servirá.", folder);
        return null;
    }
}
