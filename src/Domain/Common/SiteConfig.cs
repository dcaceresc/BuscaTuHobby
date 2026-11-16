namespace Domain.Common;

public static class SiteConfig
{
    // Carpeta de imágenes usada cuando la tabla de configuración no define FolderImage.
    public const string DefaultFolderImage = "images";

    public static bool SiteStatus { get; set; } = true;

    // Después de InitialiseConfigurationAsync esta ruta siempre es absoluta: el escritor de imágenes y el
    // middleware que las sirve la usan tal cual y ninguno de los dos depende del directorio de trabajo.
    public static string FolderImage { get; set; } = default!;

    // Única resolución de la carpeta de imágenes. Una ruta relativa se resuelve contra la raíz de contenido de
    // la aplicación y una ruta absoluta se conserva. Se aplica una sola vez, al cargar la configuración, para
    // que escribir y servir la imagen no puedan apuntar a carpetas distintas cuando el proceso arranca con un
    // directorio de trabajo diferente de la raíz de contenido (systemd, IIS en proceso).
    public static string ResolveFolderImage(string? configuredFolder, string contentRootPath)
    {
        var configured = string.IsNullOrWhiteSpace(configuredFolder) ? DefaultFolderImage : configuredFolder;
        if (Path.IsPathRooted(configured))
            return Path.GetFullPath(configured);
        if (string.IsNullOrWhiteSpace(contentRootPath))
            throw new ArgumentException("Se requiere la raíz de contenido para resolver una carpeta relativa.",
                nameof(contentRootPath));
        return Path.GetFullPath(Path.Combine(contentRootPath, configured));
    }
}
