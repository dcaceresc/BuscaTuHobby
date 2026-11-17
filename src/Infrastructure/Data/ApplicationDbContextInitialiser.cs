using Application.Common.PriceHistory;
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


public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, IUtilityService utilityService)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly ApplicationDbContext _context = context;
    private readonly IUtilityService _utilityService = utilityService;

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

        await SeedPostTypesAndPostsAsync();
        await SeedProductsAsync();
        await SeedCategoriesAsync();

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
            var user = User.Create(email, _utilityService.HashPassword(password));

            user.Update(email, true, false, null);

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

    private async Task SeedPostTypesAndPostsAsync()
    {
        if (_context.PostTypes.Any())
            return;

        var guia = PostType.Create("Guia");
        var comparativa = PostType.Create("Comparativa");
        var top = PostType.Create("Top");

        await _context.PostTypes.AddRangeAsync(guia, comparativa, top);
        await _context.SaveChangesAsync();

        var posts = new[]
        {
            Post.Create(
                "¿Que maqueta de Gundam empezar?",
                "Descubre las mejores opciones para principiantes en el mundo de las maquetas Gundam. Comparamos RX-78-2, Wing Zero y Strike Freedom.",
                guia.PostTypeId),
            Post.Create(
                "Funko Pop vs Nendoroid: ¿Cual elegir?",
                "Analizamos las diferencias entre estas populares lineas de figuras coleccionables. Precio, calidad y variedad comparados.",
                comparativa.PostTypeId),
            Post.Create(
                "LEGO Star Wars: Sets para coleccionistas",
                "Los mejores sets de LEGO Star Wars segun tu presupuesto. Desde el AT-AT hasta el Millennium Falcon UCS.",
                guia.PostTypeId),
            Post.Create(
                "Top 10 figuras de accion mas buscadas 2024",
                "Las figuras mas populares del año. Hot Toys, S.H.Figuarts y mas. Encuentra donde comprarlas al mejor precio.",
                top.PostTypeId),
        };

        posts[0].SetViewCount(3245);
        posts[1].SetViewCount(2892);
        posts[2].SetViewCount(4128);
        posts[3].SetViewCount(5673);

        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();
    }

    private async Task SeedProductsAsync()
    {
        if (_context.Products.Any())
            return;

        // Fabricantes
        var bandai = Manufacturer.Create("Bandai");
        var funko = Manufacturer.Create("Funko");
        var lego = Manufacturer.Create("LEGO");

        await _context.Manufacturers.AddRangeAsync(bandai, funko, lego);
        await _context.SaveChangesAsync();

        // Franquicias
        var gundam = Franchise.Create("Gundam");
        var marvel = Franchise.Create("Marvel");
        var starWars = Franchise.Create("Star Wars");
        var dragonBall = Franchise.Create("Dragon Ball");

        await _context.Franchises.AddRangeAsync(gundam, marvel, starWars, dragonBall);
        await _context.SaveChangesAsync();

        // Tiendas
        var amazon = Store.Create("Amazon", "https://www.amazon.com", "bi bi-shop", 1, "amazon");
        var mercadoLibre = Store.Create("MercadoLibre", "https://www.mercadolibre.cl", "bi bi-shop", 2, "mercadolibre");
        var linio = Store.Create("Linio", "https://www.linio.cl", "bi bi-shop", 3, "linio");
        var falabella = Store.Create("Falabella", "https://www.falabella.com", "bi bi-shop", 4, "falabella");
        var ripley = Store.Create("Ripley", "https://www.ripley.cl", "bi bi-shop", 5, "ripley");
        var paris = Store.Create("Paris", "https://www.paris.cl", "bi bi-shop", 6, "paris");

        // Dos tiendas reales y chilenas quedan configuradas para la ingesta: son configuración, no catálogo, así que
        // no mezclan datos inventados con ofertas reales. Sin una fila configurada la importación manual no puede
        // resolver el host y responde que la URL no corresponde a ninguna tienda.
        var irion = Store.Create("Irion", "https://irion.cl", "bi bi-shop", 7, "irion");
        irion.ConfigureConnector("shopify", "{\"currency\":\"CLP\"}");
        var luffyToys = Store.Create("LuffyToys", "https://luffytoys.cl", "bi bi-shop", 8, "luffytoys");
        luffyToys.ConfigureConnector("shopify", "{\"currency\":\"CLP\"}");

        await _context.Stores.AddRangeAsync(amazon, mercadoLibre, linio, falabella, ripley, paris, irion, luffyToys);
        await _context.SaveChangesAsync();

        // Productos. Grado, escala y número de parte completan la ficha Gunpla: el único kit del catálogo de demo
        // informa su grado real y las figuras y sets que no son maquetas quedan sin grado, sin escala y solo con su
        // número de set cuando existe. Son datos de demostración, no un catálogo real.
        var gundamRx78 = Product.Create("Gundam RX-78-2", bandai.ManufacturerId, gundam.FranchiseId, null, true, "15+", "1/144", "Maqueta para armar", new DateOnly(2024, 1, 15), GunplaGrade.HighGrade, "1/144", "HGUC-191");
        var funkoSpider = Product.Create("Funko Pop Spider-Man", funko.ManufacturerId, marvel.FranchiseId, null, true, "6+", "10cm", "Figura coleccionable", new DateOnly(2024, 3, 10));
        var legoFalcon = Product.Create("LEGO Millennium Falcon", lego.ManufacturerId, starWars.FranchiseId, null, false, "18+", "84cm", "Set de construccion", new DateOnly(2024, 5, 20), GunplaGrade.None, null, "75192");
        var goku = Product.Create("S.H.Figuarts Goku", bandai.ManufacturerId, dragonBall.FranchiseId, null, true, "15+", "14cm", "Figura articulada", new DateOnly(2024, 2, 28));

        gundamRx78.SetViewCount(1234);
        funkoSpider.SetViewCount(987);
        legoFalcon.SetViewCount(856);
        goku.SetViewCount(745);

        await _context.Products.AddRangeAsync(gundamRx78, funkoSpider, legoFalcon, goku);
        await _context.SaveChangesAsync();

        // Inventarios (relaciones producto-tienda con precios originales y descuentos). Los precios de ejemplo no cambian;
        // se agrega variedad de condición y medio de pago para que la comparación sea visible sin editar la base a mano.
        var inventories = new[]
        {
            // Gundam RX-78-2
            Inventory.Create(gundamRx78.ProductId, amazon.StoreId, 25990, 39990, 35),
            Inventory.Create(gundamRx78.ProductId, mercadoLibre.StoreId, 27990, 34990, 20, ProductCondition.Refurbished),
            Inventory.Create(gundamRx78.ProductId, linio.StoreId, 26990, 32990, 18, ProductCondition.New, "Tarjeta Linio"),
            Inventory.Create(gundamRx78.ProductId, falabella.StoreId, 28990, 36990, 22, ProductCondition.New, "Tarjeta CMR Falabella"),
            Inventory.Create(gundamRx78.ProductId, ripley.StoreId, 27490, 34990, 21),
            Inventory.Create(gundamRx78.ProductId, paris.StoreId, 26490, 33490, 21),

            // Funko Pop Spider-Man
            Inventory.Create(funkoSpider.ProductId, amazon.StoreId, 12990, 16990, 24),
            Inventory.Create(funkoSpider.ProductId, falabella.StoreId, 13990, 17990, 22),
            Inventory.Create(funkoSpider.ProductId, ripley.StoreId, 14490, 17990, 19, ProductCondition.Used),
            Inventory.Create(funkoSpider.ProductId, mercadoLibre.StoreId, 11990, 15990, 25),
            Inventory.Create(funkoSpider.ProductId, linio.StoreId, 13490, 17490, 23, ProductCondition.New, "Tarjeta Linio"),
            Inventory.Create(funkoSpider.ProductId, paris.StoreId, 12490, 14990, 17),

            // LEGO Millennium Falcon
            Inventory.Create(legoFalcon.ProductId, amazon.StoreId, 899990, 1099990, 18),
            Inventory.Create(legoFalcon.ProductId, paris.StoreId, 949990, 1199990, 21),
            Inventory.Create(legoFalcon.ProductId, mercadoLibre.StoreId, 879990, 1149990, 23, ProductCondition.Used),
            Inventory.Create(legoFalcon.ProductId, falabella.StoreId, 929990, 1299990, 28, ProductCondition.New, "Tarjeta CMR Falabella"),
            Inventory.Create(legoFalcon.ProductId, ripley.StoreId, 919990, 1199990, 23),
            Inventory.Create(legoFalcon.ProductId, linio.StoreId, 889990, 1249990, 29),

            // S.H.Figuarts Goku
            Inventory.Create(goku.ProductId, linio.StoreId, 45990, 69990, 34),
            Inventory.Create(goku.ProductId, mercadoLibre.StoreId, 43990, 54990, 20),
            Inventory.Create(goku.ProductId, ripley.StoreId, 47990, 59990, 20),
            Inventory.Create(goku.ProductId, amazon.StoreId, 44990, 74990, 40, ProductCondition.Refurbished),
            Inventory.Create(goku.ProductId, falabella.StoreId, 46990, 62990, 25),
            Inventory.Create(goku.ProductId, paris.StoreId, 48990, 64990, 25),
        };

        // La frescura es parte de lo que la tabla de ofertas debe mostrar: con fecha de observación nula la celda
        // queda vacía, así que el seed reparte observaciones recientes en lugar de dejar el campo sin dato.
        var observedAt = DateTime.UtcNow.AddMinutes(-25);
        foreach (var inventory in inventories)
        {
            inventory.MarkObserved(observedAt);
            observedAt = observedAt.AddMinutes(-37);
        }

        await _context.Inventories.AddRangeAsync(inventories);
        await _context.SaveChangesAsync();

        // Cada oferta sembrada inicia su serie de precios con el punto de línea base con el que se creó, así que
        // una base de datos de desarrollo recién sembrada muestra una serie real en lugar de un gráfico vacío.
        foreach (var inventory in inventories)
            InventoryPriceHistoryRecorder.RecordBaseline(_context, inventory);
        await _context.SaveChangesAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        if (_context.Categories.Any())
            return;

        // Categorías
        var figurasDeAccion = Category.Create("Figuras de Acción", "bi bi-person-standing", 1, "figuras-de-accion");
        var maquetas = Category.Create("Maquetas", "bi bi-tools", 2, "maquetas");
        var coleccionables = Category.Create("Coleccionables", "bi bi-gem", 3, "coleccionables");
        var legoCategory = Category.Create("LEGO", "bi bi-bricks", 4, "lego");

        await _context.Categories.AddRangeAsync(figurasDeAccion, maquetas, coleccionables, legoCategory);
        await _context.SaveChangesAsync();

        // Obtener los productos sembrados
        var products = await _context.Products.ToListAsync();
        var gundamRx78 = products.First(p => p.ProductName == "Gundam RX-78-2");
        var funkoSpider = products.First(p => p.ProductName == "Funko Pop Spider-Man");
        var legoFalcon = products.First(p => p.ProductName == "LEGO Millennium Falcon");
        var goku = products.First(p => p.ProductName == "S.H.Figuarts Goku");

        // Relaciones de ProductCategory
        // Figuras de Acción: Funko Spider-Man, Goku, Gundam RX-78-2
        // Maquetas: Gundam RX-78-2, LEGO Falcon
        // Coleccionables: Funko Spider-Man, Goku
        // LEGO: LEGO Falcon
        var productCategories = new[]
        {
            ProductCategory.Create(funkoSpider.ProductId, figurasDeAccion.CategoryId),
            ProductCategory.Create(goku.ProductId, figurasDeAccion.CategoryId),
            ProductCategory.Create(gundamRx78.ProductId, figurasDeAccion.CategoryId),

            ProductCategory.Create(gundamRx78.ProductId, maquetas.CategoryId),
            ProductCategory.Create(legoFalcon.ProductId, maquetas.CategoryId),

            ProductCategory.Create(funkoSpider.ProductId, coleccionables.CategoryId),
            ProductCategory.Create(goku.ProductId, coleccionables.CategoryId),

            ProductCategory.Create(legoFalcon.ProductId, legoCategory.CategoryId),
        };

        await _context.ProductCategories.AddRangeAsync(productCategories);
        await _context.SaveChangesAsync();
    }
}
