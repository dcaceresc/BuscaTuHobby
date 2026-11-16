using System.Reflection;
using Application.Common.Interfaces;
using Application.Dashboard.Queries.GetBestDeals;
using Application.Dashboard.Queries.GetPopularCategories;
using Application.Dashboard.Queries.GetProductPriceHistory;
using Application.Dashboard.Queries.GetRecentActivity;
using Application.Maintainer.Products.Queries.GetProductById;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

// Comprobaciones de veracidad de los widgets del dashboard, del historial de precios y de la carpeta de imágenes.
// Todo aquí se ejecuta contra dobles en memoria, generación de SQL sin conexión y una carpeta temporal: sin base de
// datos, sin HTTP y sin arrancar la API.
internal static class FidelityChecks
{
    private const string StructuralConnection =
        "Server=unused;Database=StructuralOnly;Integrated Security=true;TrustServerCertificate=true";

    public static async Task Run(Action<bool, string> check)
    {
        var services = new ApiResponseService();
        await PopularCategories(check, services);
        await RecentActivityFeed(check, services);
        await PriceHistorySeries(check, services);
        await BestDeals(check, services);
        await OfferFreshness(check);
        await ProductByIdImages(check, services);
        SqlTranslation(check);
        ImagesFolderMapping(check);
        await ImagesFolderResolution(check);
    }

    private static async Task PopularCategories(Action<bool, string> check, IApiResponseService services)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;

        Product AddProduct(string name, bool active = true)
        {
            var product = Product.Create(name, Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
            if (!active) product.ToggleActive();
            fake.Products.Add(product);
            return product;
        }
        Category AddCategory(string name, int count, bool active = true)
        {
            var category = Category.Create(name, "bi bi-box", 0, name.ToLowerInvariant());
            Set(category, nameof(Category.ProductCategories), new List<ProductCategory>());
            if (!active) category.ToggleActive();
            fake.Categories.Add(category);
            for (var index = 0; index < count; index++)
                Assign(category, AddProduct(name + " " + index));
            return category;
        }

        var big = AddCategory("Grande", 6);
        AddCategory("Enorme", 4);
        AddCategory("Medio", 3);
        AddCategory("Pequeña", 2);
        AddCategory("Diminuta", 1);
        AddCategory("Inactiva", 10, active: false);
        Assign(big, AddProduct("Retirado", active: false));

        var stored = string.Join(",", fake.Categories.Select(c => c.CategoryId));

        var response = await new GetPopularCategoriesHandler(context, services).Handle(new GetPopularCategories(), default);
        var rows = response.Data ?? [];
        check(response.Success && response.Message is null && rows.Count == 4,
            "Popular categories keep the exact four-row success envelope");
        check(rows.Select(r => r.CategoryName).SequenceEqual(new[] { "Grande", "Enorme", "Medio", "Pequeña" }) &&
            rows.Select(r => r.ProductCount).SequenceEqual(new[] { 6, 4, 3, 2 }),
            "Popular categories keep the same four rows and counts, ignoring inactive categories and products");
        var expected = new[] { 6, 4, 3, 2 }
            .Select(count => (int)Math.Round((double)count / 16 * 100)).ToArray();
        check(rows.Select(r => r.Percentage).SequenceEqual(expected) && rows.Sum(r => r.Percentage) < 100,
            "Percentages divide by every active category, so a strict top-four subset adds up to less than 100");
        check(rows.All(r => r.Percentage is >= 0 and <= 100),
            "Rounding never produces a percentage above 100");
        check(string.Join(",", fake.Categories.Select(c => c.CategoryId)) == stored && fake.Saves == 0,
            "Popular categories leave stored rows and save counters unchanged");

        // Cuando el límite cubre todas las categorías activas, la participación honesta sigue siendo 100.
        var complete = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var completeFake = (InspectionContext)(object)complete;
        foreach (var (name, count) in new[] { ("Uno", 6), ("Dos", 4) })
        {
            var category = Category.Create(name, "bi bi-box", 0, name.ToLowerInvariant());
            Set(category, nameof(Category.ProductCategories), new List<ProductCategory>());
            completeFake.Categories.Add(category);
            for (var index = 0; index < count; index++)
            {
                var product = Product.Create(name + " " + index, Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
                completeFake.Products.Add(product);
                Assign(category, product);
            }
        }
        var all = await new GetPopularCategoriesHandler(complete, services).Handle(new GetPopularCategories(), default);
        check(all.Success && all.Message is null && all.Data is not null &&
            all.Data.Select(r => r.Percentage).SequenceEqual(new[] { 60, 40 }) && all.Data.Sum(r => r.Percentage) == 100,
            "A top four that covers every active category still adds up to 100");

        // Cinco categorías activas y no vacías, ordenadas de mayor a menor, con el límite de cuatro filas: cada fila
        // es una parte pequeña del total y la primera debe reflejar el total, no la suma de las cuatro mostradas.
        var wide = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var wideFake = (InspectionContext)(object)wide;
        foreach (var (name, count) in new[] { ("Cincuenta", 60), ("Cincuenta y cinco", 55), ("Cuarenta y cinco", 45), ("Treinta", 35), ("Veinte", 30) })
        {
            var category = Category.Create(name, "bi bi-box", 0, name.ToLowerInvariant().Replace(' ', '-'));
            Set(category, nameof(Category.ProductCategories), new List<ProductCategory>());
            wideFake.Categories.Add(category);
            for (var index = 0; index < count; index++)
            {
                var product = Product.Create(name + " " + index, Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
                wideFake.Products.Add(product);
                Assign(category, product);
            }
        }
        var share = await new GetPopularCategoriesHandler(wide, services).Handle(new GetPopularCategories(), default);
        var shareRows = share.Data ?? [];
        // Total honesto: 225 productos enlazados en las cinco categorías activas.
        var topFourSum = 60 + 55 + 45 + 35;
        check(share.Success && share.Message is null && shareRows.Count == 4 &&
            shareRows.Select(r => r.ProductCount).SequenceEqual(new[] { 60, 55, 45, 35 }),
            "Five non-empty active categories still produce exactly four rows ordered by product count");
        check(shareRows.Count == 4 && shareRows[0].Percentage == (int)Math.Round(60d / 225 * 100) &&
            shareRows[0].Percentage != (int)Math.Round(60d / topFourSum * 100) &&
            shareRows[0].Percentage < 50,
            "The first row divides by every active category, not by the sum of the four shown rows");
        check(shareRows.Select(r => r.Percentage).SequenceEqual(new[] { 27, 24, 20, 16 }) &&
            shareRows.Sum(r => r.Percentage) < 100,
            "Each shown row is a small part of the whole, so the four rows never add up to 100");

        // Un mismo producto en dos categorías: dos filas y ninguna llega al 100 %.
        var shared = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var sharedFake = (InspectionContext)(object)shared;
        var twice = Product.Create("Dos veces", Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
        sharedFake.Products.Add(twice);
        foreach (var name in new[] { "Primera", "Segunda" })
        {
            var category = Category.Create(name, "bi bi-box", 0, name.ToLowerInvariant());
            Set(category, nameof(Category.ProductCategories), new List<ProductCategory>());
            sharedFake.Categories.Add(category);
            Assign(category, twice);
        }
        var sharedRows = (await new GetPopularCategoriesHandler(shared, services)
            .Handle(new GetPopularCategories(), default)).Data ?? [];
        check(sharedRows.Count == 2 && sharedRows.All(r => r.ProductCount == 1) &&
            sharedRows.Select(r => r.Percentage).SequenceEqual(new[] { 50, 50 }) &&
            sharedRows.All(r => r.Percentage < 100),
            "A product linked to two categories yields two rows, each below 100 percent");
    }

    private static async Task RecentActivityFeed(Action<bool, string> check, IApiResponseService services)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;
        var start = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

        Product AddProduct(string name, bool active, double hours)
        {
            var product = Product.Create(name, Guid.NewGuid(), Guid.NewGuid(), null, false, "", "", "", default);
            product.Created = start.AddHours(-hours);
            if (!active) product.ToggleActive();
            fake.Products.Add(product);
            return product;
        }
        Store AddStore(string name, bool active, double hours)
        {
            var store = Store.Create(name, "https://example.test/", "icon", 0, name.ToLowerInvariant().Replace(' ', '-'));
            store.Created = start.AddHours(-hours);
            if (!active) store.ToggleActive();
            fake.Stores.Add(store);
            return store;
        }
        Inventory AddOffer(Product owner, Store store, int discount, bool active, double hours)
        {
            var offer = Inventory.Create(owner.ProductId, store.StoreId, 100, 0, discount);
            Set(offer, nameof(Inventory.Product), owner);
            Set(offer, nameof(Inventory.Store), store);
            offer.Created = start.AddHours(-hours);
            if (!active) offer.ToggleActive();
            fake.Rows.Add(offer);
            return offer;
        }
        Inventory AddImportedOffer(Product owner, Store store, int price, int originalPrice, int discount,
            bool available, double hours)
        {
            var offer = Inventory.CreateImportedOffer(owner.ProductId, store.StoreId, price, originalPrice, discount,
                "https://irion.cl/products/model", "123", available, start.AddHours(-hours));
            Set(offer, nameof(Inventory.Product), owner);
            Set(offer, nameof(Inventory.Store), store);
            offer.Created = start.AddHours(-hours);
            fake.Rows.Add(offer);
            return offer;
        }
        Post AddPost(string title, bool active, double hours)
        {
            var post = Post.Create(title, "Contenido", Guid.NewGuid());
            post.Created = start.AddHours(-hours);
            if (!active) post.ToggleActive();
            fake.Posts.Add(post);
            return post;
        }

        var activeProduct = AddProduct("Activo", true, 1);
        var retiredProduct = AddProduct("Retirado", false, 2);
        var openStore = AddStore("Tienda Viva", true, 3);
        var closedStore = AddStore("Tienda Cerrada", false, 4);
        var discounted = AddOffer(activeProduct, openStore, 15, true, 5);
        var plain = AddOffer(activeProduct, openStore, 0, true, 6);
        AddOffer(activeProduct, openStore, 25, false, 7);
        AddOffer(retiredProduct, openStore, 30, true, 8);
        AddOffer(activeProduct, closedStore, 35, true, 9);
        // Misma tienda y mismo producto: una oferta disponible se anuncia y la no disponible no.
        var available = AddImportedOffer(activeProduct, openStore, 100, 200, 50, true, 5.5);
        var soldOut = AddImportedOffer(activeProduct, openStore, 75, 100, 25, false, 4.5);
        AddPost("Guía vigente", true, 10);
        AddPost("Guía retirada", false, 11);

        var response = await new GetRecentActivityHandler(context, services).Handle(new GetRecentActivity(), default);
        var rows = response.Data ?? [];
        check(response.Success && response.Message is null && rows.Count == 6,
            "Recent activity returns only the eligible rows of every source");
        check(rows.Select(r => (r.ActivityType, r.Title, r.Description)).SequenceEqual(new[]
            {
                ("Producto", "Nuevo producto agregado", "Activo agregado a la plataforma"),
                ("Tienda", "Nueva tienda afiliada", "Tienda Viva se unió a la plataforma"),
                ("Oferta", "Nueva oferta disponible", "Activo en Tienda Viva con 15% descuento"),
                ("Oferta", "Nueva oferta disponible", "Activo en Tienda Viva con 50% descuento"),
                ("Oferta", "Precio actualizado", "Activo en Tienda Viva"),
                ("Publicación", "Nueva publicación", "Guía vigente")
            }),
            "Every existing activity type, title and description string is preserved unchanged");
        check(rows.Zip(rows.Skip(1)).All(pair => pair.First.CreatedAt >= pair.Second.CreatedAt),
            "Activity stays ordered newest first");
        check(rows.All(r => !r.Description.Contains("Retirado") && !r.Description.Contains("Tienda Cerrada") &&
            !r.Description.Contains("25% descuento") && !r.Description.Contains("30% descuento") &&
            !r.Description.Contains("35% descuento") && r.Description != "Guía retirada"),
            "Inactive products, disabled offers, closed stores and inactive posts never appear as activity");
        check(rows.Any(r => r.Description == "Activo en Tienda Viva con 50% descuento") &&
            rows.All(r => !r.Description.Contains("25% descuento")),
            "An unavailable offer is excluded from the feed while an available one from the same store is kept");
        check(discounted.InventoryId != Guid.Empty && plain.InventoryId != Guid.Empty &&
            available.InventoryId != soldOut.InventoryId,
            "Both eligible offers of the same product and store are reported");

        for (var index = 0; index < 6; index++) AddProduct("Extra " + index, true, 20 + index);
        var merged = await new GetRecentActivityHandler(context, services).Handle(new GetRecentActivity(), default);
        var mergedRows = merged.Data ?? [];
        check(merged.Success && mergedRows.Count(r => r.ActivityType == "Producto") == 5 && mergedRows.Count <= 10 &&
            mergedRows.Zip(mergedRows.Skip(1)).All(pair => pair.First.CreatedAt >= pair.Second.CreatedAt),
            "The five-row-per-source cap and the newest-first merge are preserved");
        check(fake.Saves == 0, "Recent activity saves nothing");
    }

    private static async Task PriceHistorySeries(Action<bool, string> check, IApiResponseService services)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;

        var manufacturer = Manufacturer.Create("Bandai");
        var franchise = Franchise.Create("Gundam");
        var product = Product.Create("RX-78-2", manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "8+", "18cm", "Maqueta", new DateOnly(2024, 1, 15));
        var other = Product.Create("Otro", manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "8+", "10cm", "Otra", new DateOnly(2024, 2, 20));
        var onlyIneligible = Product.Create("Solo inelegible", manufacturer.ManufacturerId, franchise.FranchiseId,
            null, false, "8+", "10cm", "Solo inelegible", new DateOnly(2024, 3, 1));
        fake.Products.Add(product);
        fake.Products.Add(other);
        fake.Products.Add(onlyIneligible);

        var open = Store.Create("Abierta", "https://abierta.cl", "icon", 1, "abierta");
        var closed = Store.Create("Cerrada", "https://cerrada.cl", "icon", 2, "cerrada");
        closed.ToggleActive();
        fake.Stores.Add(open);
        fake.Stores.Add(closed);

        Inventory AddOffer(Product owner, Store store, int price)
        {
            var offer = Inventory.Create(owner.ProductId, store.StoreId, price, price, 0);
            Set(offer, nameof(Inventory.Product), owner);
            Set(offer, nameof(Inventory.Store), store);
            fake.Rows.Add(offer);
            return offer;
        }

        var eligible = AddOffer(product, open, 20000);
        var alsoEligible = AddOffer(product, open, 23000);
        Set(alsoEligible, nameof(Inventory.Availability), (bool?)true);
        var disabledOffer = AddOffer(product, open, 1000);
        disabledOffer.ToggleActive();
        var closedStoreOffer = AddOffer(product, closed, 500);
        var unavailable = AddOffer(product, open, 400);
        Set(unavailable, nameof(Inventory.Availability), (bool?)false);
        var foreign = AddOffer(other, open, 77);
        var ineligibleOnly = AddOffer(onlyIneligible, open, 300);
        ineligibleOnly.ToggleActive();

        var day = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc);
        AddPoint(fake, eligible, 20000, day);
        AddPoint(fake, alsoEligible, 23000, day.AddHours(2));
        AddPoint(fake, disabledOffer, 1000, day.AddHours(3));
        AddPoint(fake, closedStoreOffer, 500, day.AddHours(4));
        AddPoint(fake, unavailable, 400, day.AddHours(5));
        AddPoint(fake, disabledOffer, 900, day.AddDays(1));
        AddPoint(fake, eligible, 19990, day.AddDays(2));
        AddPoint(fake, foreign, 77, day);
        AddPoint(fake, ineligibleOnly, 300, day);

        var pointsBefore = fake.Histories.Count;
        var response = await new GetProductPriceHistoryHandler(context, services).Handle(new(product.ProductId), default);
        var points = response.Data?.Points ?? [];
        check(response.Success && response.Data is not null && response.Data.HasHistory && points.Count == 3,
            "The series keeps every recorded day of the product, whatever the offer's current state");
        check(points.Count == 3 && points[0].Date == day.Date && points[0].MinPrice == 400 &&
            points[0].MaxPrice == 23000 && points[0].OfferCount == 5 &&
            points[1].Date == day.AddDays(1).Date && points[1].MinPrice == 900 &&
            points[1].MaxPrice == 900 && points[1].OfferCount == 1 &&
            points[2].Date == day.AddDays(2).Date && points[2].MinPrice == 19990 &&
            points[2].MaxPrice == 19990 && points[2].OfferCount == 1,
            "Points from a now-disabled offer, a closed store and an unavailable offer still draw the chart");
        check(points.Any(p => p.MinPrice == 400 && p.MaxPrice == 23000) &&
            points.All(p => p.MinPrice is not (77 or 300) && p.MaxPrice is not (77 or 300)),
            "The series keeps the prices of ineligible offers but never another product's observation");

        var hidden = await new GetProductPriceHistoryHandler(context, services).Handle(new(onlyIneligible.ProductId), default);
        check(hidden.Success && hidden.Data is not null && hidden.Data.HasHistory &&
            hidden.Data.Points.Count == 1 && hidden.Data.Points[0].MinPrice == 300 &&
            hidden.Data.Points[0].MaxPrice == 300 && hidden.Data.Points[0].OfferCount == 1,
            "A product whose only offer is now disabled still reports what it cost instead of claiming no prices");
        check(fake.Histories.Count == pointsBefore && fake.Saves == 0,
            "Reading the series appends no observation and saves nothing");

        // La ventana de días sigue recortando la serie y no reintroduce el filtro de elegibilidad.
        var windowContext = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var windowFake = (InspectionContext)(object)windowContext;
        var watched = Product.Create("Vigilado", manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "8+", "10cm", "Otra", new DateOnly(2024, 4, 1));
        windowFake.Products.Add(watched);
        var closedShop = Store.Create("Clausurada", "https://clausurada.cl", "icon", 3, "clausurada");
        closedShop.ToggleActive();
        windowFake.Stores.Add(closedShop);
        var stale = Inventory.Create(watched.ProductId, closedShop.StoreId, 800, 800, 0);
        Set(stale, nameof(Inventory.Product), watched);
        Set(stale, nameof(Inventory.Store), closedShop);
        stale.ToggleActive();
        windowFake.Rows.Add(stale);
        var now = DateTime.UtcNow;
        AddPoint(windowFake, stale, 900, now.AddDays(-400));
        AddPoint(windowFake, stale, 800, now.AddDays(-2));

        var complete = await new GetProductPriceHistoryHandler(windowContext, services).Handle(new(watched.ProductId), default);
        var windowed = await new GetProductPriceHistoryHandler(windowContext, services)
            .Handle(new(watched.ProductId, 30), default);
        check(complete.Success && complete.Data is { HasHistory: true } && complete.Data.Points.Count == 2,
            "Without a day window a disabled offer in a closed store still draws both recorded points");
        check(windowed.Success && windowed.Data is { HasHistory: true } && windowed.Data.Points.Count == 1 &&
            windowed.Data.Points[0].MinPrice == 800 && windowed.Data.Points[0].MaxPrice == 800,
            "The days window still trims the series to the requested recent days");
    }

    private static async Task BestDeals(Action<bool, string> check, IApiResponseService services)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;
        var manufacturer = Manufacturer.Create("Bandai");
        var franchise = Franchise.Create("Gundam");
        var product = Product.Create("RX-78-2", manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "8+", "18cm", "Maqueta", new DateOnly(2024, 1, 15));
        product.SetViewCount(7);
        var store = Store.Create("Tienda", "https://tienda.cl", "icon", 1, "tienda");
        fake.Products.Add(product);
        fake.Stores.Add(store);

        Inventory AddOffer(int price, int discount)
        {
            var offer = Inventory.Create(product.ProductId, store.StoreId, price, price, discount);
            Set(offer, nameof(Inventory.Product), product);
            Set(offer, nameof(Inventory.Store), store);
            fake.Rows.Add(offer);
            return offer;
        }

        var best = AddOffer(100, 30);
        AddOffer(200, 10);

        var response = await new GetBestDealsHandler(context, services).Handle(new GetBestDeals(), default);
        var deals = response.Data ?? [];
        check(response.Success && response.Message is null && deals.Count == 2 &&
            deals[0].InventoryId == best.InventoryId,
            "Highlighted offers keep their envelope and discount ordering");
        check(deals.All(deal => deal.ProductId == product.ProductId),
            "Every highlighted offer carries the product identifier so it can link to the product");
        check(deals[0].ProductName == "RX-78-2" && deals[0].StoreName == "Tienda" &&
            deals[0].DiscountPercentage == 30 && deals[0].ProductViewCount == 7,
            "The added product identifier leaves every existing deal field unchanged");
        check(fake.Saves == 0, "Reading highlighted offers saves nothing");
    }

    private static async Task ProductByIdImages(Action<bool, string> check, IApiResponseService services)
    {
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;
        var manufacturer = Manufacturer.Create("Bandai");
        var franchise = Franchise.Create("Gundam");
        var category = Category.Create("Maquetas", "bi bi-tools", 1, "maquetas");
        var product = Product.Create("RX-78-2", manufacturer.ManufacturerId, franchise.FranchiseId, null, true,
            "8+", "18cm", "Maqueta a escala", new DateOnly(2024, 1, 15));
        var link = product.AssignCategory(category.CategoryId);
        Set(link, nameof(ProductCategory.Product), product);
        Set(link, nameof(ProductCategory.Category), category);
        Set(product, nameof(Product.ProductCategories), new List<ProductCategory> { link });
        // Deliberadamente desordenadas: la proyección debe ordenar por ProductImageOrder.
        var second = product.AssignImage(2);
        var first = product.AssignImage(1);
        Set(product, nameof(Product.ProductImages), new List<ProductImage> { second, first });
        fake.Products.Add(product);

        var handler = new GetProductByIdHandler(context, services);
        var response = await handler.Handle(new(product.ProductId), default);
        var data = response.Data;
        check(response.Success && response.Message is null && data is not null &&
            data.ProductImages.SequenceEqual(new[]
            {
                $"/images/{first.ProductImageId}.jpg", $"/images/{second.ProductImageId}.jpg"
            }),
            "Product images are published as the request path /images/{id}.jpg in image order");
        check(data is not null && data.ProductImages.All(url =>
                url.StartsWith("/images/", StringComparison.Ordinal) &&
                url.EndsWith(".jpg", StringComparison.Ordinal) &&
                !url.StartsWith("images/", StringComparison.Ordinal)),
            "No physical folder name leaks into the published image URL");
        check(data is not null && data.ProductName == "RX-78-2" &&
            data.CategoryIds.SequenceEqual(new[] { category.CategoryId }) &&
            data.ProductReleaseDate == "2024-01-15" && data.ProductSize == "18cm" &&
            data.ProductTargetAge == "8+" && data.ProductHasBase && data.ProductDescription == "Maqueta a escala" &&
            data.ManufacturerId == manufacturer.ManufacturerId && data.FranchiseId == franchise.FranchiseId &&
            data.SerieId is null,
            "The maintenance product detail fields stay unchanged");
        var missing = await handler.Handle(new(Guid.NewGuid()), default);
        check(!missing.Success && missing.Message == "An error occurred while processing your request" &&
            missing.Data is null,
            "An unknown product keeps its existing failure envelope");
    }

    // Prueba solo de traducción: el proveedor genera SQL sin conexión, no se abre ni se ejecuta ninguna conexión.
    private static void SqlTranslation(Action<bool, string> check)
    {
        using var context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(StructuralConnection).Options);
        var productId = Guid.NewGuid();

        void CheckSql(string message, Func<string> translate, params string[] fragments)
        {
            try
            {
                var sql = translate();
                check(fragments.All(fragment => sql.Contains(fragment, StringComparison.OrdinalIgnoreCase)), message);
            }
            catch (Exception exception)
            {
                check(false, message + " (translation threw " + exception.GetType().Name + ": " + exception.Message + ")");
            }
        }

        CheckSql("The popular-categories counts and top four translate to SQL Server",
            () => context.Categories
                .Where(c => c.IsActive)
                .Select(c => new { c.CategoryName, ProductCount = c.ProductCategories.Count(pc => pc.Product.IsActive) })
                .Where(c => c.ProductCount > 0)
                .OrderByDescending(c => c.ProductCount)
                .Take(4)
                .ToQueryString(),
            "ProductCategories", "Products", "TOP");
        CheckSql("The popular-categories total over every active category translates to SQL Server",
            () => context.Categories
                .Where(c => c.IsActive)
                .SelectMany(c => c.ProductCategories)
                .Where(pc => pc.Product.IsActive)
                .ToQueryString(),
            "ProductCategories", "Products");
        CheckSql("The recent-activity offer eligibility predicate translates to SQL Server",
            () => context.Inventories
                .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                .OrderByDescending(i => i.Created)
                .Take(5)
                .Select(i => new RecentActivityDto
                {
                    ActivityType = "Oferta",
                    Title = i.DiscountPercentage > 0 ? "Nueva oferta disponible" : "Precio actualizado",
                    Description = i.Product.ProductName + " en " + i.Store.StoreName,
                    CreatedAt = i.Created
                })
                .ToQueryString(),
            "Inventories", "Stores", "Products", "Availability", "ORDER BY");
        CheckSql("The recent-activity post filter translates to SQL Server",
            () => context.Posts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Created)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    ActivityType = "Publicación",
                    Title = "Nueva publicación",
                    Description = p.PostTitle,
                    CreatedAt = p.Created
                })
                .ToQueryString(),
            "Posts", "IsActive");
        // El historial de precios se filtra solo por producto y por ventana temporal: la asimetría con la tabla de
        // ofertas es deliberada y la interfaz debe explicarla.
        CheckSql("The price-history product filter and the daily grouping translate to SQL Server",
            () => context.InventoryPriceHistories
                .Where(h => h.Inventory.ProductId == productId)
                .GroupBy(h => h.ObservedAtUtc.Date)
                .Select(g => new PricePointDto
                {
                    Date = g.Key,
                    MinPrice = g.Min(h => h.Price),
                    MaxPrice = g.Max(h => h.Price),
                    OfferCount = g.Select(h => h.InventoryId).Distinct().Count()
                })
                .OrderBy(p => p.Date)
                .ToQueryString(),
            "InventoryPriceHistories", "GROUP BY", "MIN(", "MAX(", "Inventories");
        CheckSql("The maintenance product-by-id projection translates to SQL Server",
            () => context.Products
                .Where(x => x.ProductId == productId)
                .Select(x => new
                {
                    Detail = new ProductVM
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        CategoryIds = x.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                        FranchiseId = x.FranchiseId,
                        ManufacturerId = x.ManufacturerId,
                        SerieId = x.SerieId,
                        ProductDescription = x.ProductDescription,
                        ProductHasBase = x.ProductHasBase,
                        ProductReleaseDate = x.ProductReleaseDate.ToString("yyyy-MM-dd"),
                        ProductTargetAge = x.ProductTargetAge,
                        ProductSize = x.ProductSize
                    },
                    ImageIds = x.ProductImages
                        .OrderBy(pi => pi.ProductImageOrder)
                        .Select(pi => pi.ProductImageId)
                        .ToList()
                })
                .ToQueryString(),
            "ProductCategories", "ProductImages");
    }

    // Solo estructural: Program.cs es la definición del pipeline y esta suite no puede hospedar HTTP.
    // La fuente verifica que el mapeo de la carpeta de imágenes existe, se crea si falta y está en el lugar correcto.
    private static void ImagesFolderMapping(Action<bool, string> check)
    {
        var source = File.ReadAllText(Path.Combine("src", "Web", "WebAPI", "Program.cs"));
        int Index(string fragment) => source.IndexOf(fragment, StringComparison.Ordinal);

        check(Index("app.UseStaticFiles();") > 0,
            "The existing web-root static file call is still in the pipeline");
        check(Index("RequestPath = \"/images\"") > 0 &&
            source.Contains("new PhysicalFileProvider(", StringComparison.Ordinal) &&
            source.Contains("SiteConfig.FolderImage", StringComparison.Ordinal),
            "The configured folder is served as static content under the request path /images");
        check(Index("RequestPath = \"/images\"") > Index("await app.InitialiseConfigurationAsync()") &&
            Index("RequestPath = \"/images\"") < Index("app.MapFallbackToFile("),
            "The image mapping sits after the configuration load and before the SPA fallback");
        check(source.Contains("Directory.CreateDirectory(", StringComparison.Ordinal) &&
            source.Contains("Path.IsPathRooted(", StringComparison.Ordinal) &&
            source.Contains("environment.ContentRootPath", StringComparison.Ordinal),
            "The image folder is created when missing and resolved against the content root unless it is absolute");
        check(source.Contains("return null;", StringComparison.Ordinal) &&
            source.Contains("LogWarning", StringComparison.Ordinal),
            "A folder that cannot be created is logged and skipped instead of aborting startup");
    }

    // Comprueba la única resolución de la carpeta de imágenes y el escritor real del archivo. No prueba que una
    // petición HTTP sirva la imagen ni que la raíz de contenido de un despliegue real difiera de su directorio de
    // trabajo: aquí solo se compara la regla compartida con una carpeta temporal fuera del repositorio.
    private static async Task ImagesFolderResolution(Action<bool, string> check)
    {
        var root = Path.Combine(Path.GetTempPath(), "fidelity-images-" + Guid.NewGuid().ToString("N"));
        var contentRoot = Path.GetFullPath(Path.Combine(root, "content"));
        var absoluteFolder = Path.GetFullPath(Path.Combine(root, "absolute-images"));
        var workingDirectory = Path.GetFullPath(Path.Combine(root, "elsewhere"));
        Directory.CreateDirectory(contentRoot);
        Directory.CreateDirectory(absoluteFolder);
        Directory.CreateDirectory(workingDirectory);

        var originalFolder = SiteConfig.FolderImage;
        var originalDirectory = Directory.GetCurrentDirectory();
        try
        {
            var relative = SiteConfig.ResolveFolderImage("images", contentRoot);
            check(Path.IsPathRooted(relative) && relative == Path.Combine(contentRoot, "images"),
                "A relative configured folder resolves to an absolute path under the content root");
            check(SiteConfig.ResolveFolderImage(null, contentRoot) == relative &&
                SiteConfig.ResolveFolderImage("  ", contentRoot) == relative,
                "A missing or blank configuration keeps the same images default");
            var absolute = SiteConfig.ResolveFolderImage(absoluteFolder, contentRoot);
            check(Path.IsPathRooted(absolute) && absolute == absoluteFolder && absolute != relative,
                "An absolute configured folder is kept exactly as an absolute path");

            // La regla del lector es la de Program.cs: una ruta absoluta se usa tal cual y una relativa se resuelve
            // contra la raíz de contenido. Con el valor ya resuelto, ambas ramas devuelven la carpeta del escritor.
            string Reader(string configured) =>
                Path.IsPathRooted(configured) ? configured : Path.Combine(contentRoot, configured);

            SiteConfig.FolderImage = absolute;
            var imageId = Guid.NewGuid();
            Directory.SetCurrentDirectory(workingDirectory);
            await new UtilityService().SaveImagen(Convert.ToBase64String([1, 2, 3, 4]), imageId);
            Directory.SetCurrentDirectory(originalDirectory);
            check(File.Exists(Path.Combine(absoluteFolder, imageId + ".jpg")),
                "The image writer stores the file in the configured absolute folder");
            check(!File.Exists(Path.Combine(workingDirectory, imageId + ".jpg")),
                "The image writer ignores the process working directory");
            check(Reader(SiteConfig.FolderImage) == Path.GetFullPath(absoluteFolder) &&
                Reader(SiteConfig.FolderImage) == Path.GetDirectoryName(
                    Path.Combine(absoluteFolder, imageId + ".jpg")),
                "The reader resolves the same absolute folder the writer wrote into");

            var initializer = File.ReadAllText(Path.Combine("src", "Infrastructure", "Configuration", "ConfigurationInitializer.cs"));
            check(initializer.Contains("app.Environment.ContentRootPath", StringComparison.Ordinal) &&
                initializer.Contains("SiteConfig.ResolveFolderImage(", StringComparison.Ordinal) &&
                !initializer.Contains("SiteConfig.FolderImage = configurations", StringComparison.Ordinal),
                "The configuration load is the only place that resolves the folder, against the content root");
            var utility = File.ReadAllText(Path.Combine("src", "Infrastructure", "Services", "UtilityService.cs"));
            check(utility.Contains("SiteConfig.FolderImage", StringComparison.Ordinal) &&
                !utility.Contains("ContentRootPath", StringComparison.Ordinal),
                "The image writer uses the resolved folder and never resolves it a second time");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            SiteConfig.FolderImage = originalFolder;
            try
            {
                Directory.Delete(root, true);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                check(false, "The temporary image folders could not be removed: " + root);
            }
        }
    }

    // La frescura de una oferta creada a mano es un dato obligatorio: sin ella la tabla de ofertas no muestra fecha.
    private static Task OfferFreshness(Action<bool, string> check)
    {
        var manufacturer = Manufacturer.Create("Bandai");
        var franchise = Franchise.Create("Gundam");
        var product = Product.Create("RG 1/144 RX-78-2", manufacturer.ManufacturerId, franchise.FranchiseId, null,
            false, "8+", "13cm", "Maqueta", new DateOnly(2024, 2, 1));
        var store = Store.Create("Tienda", "https://tienda.cl", "icon", 1, "tienda");
        var offer = Inventory.Create(product.ProductId, store.StoreId, 1000, 1000, 0);

        var observedAt = new DateTime(2026, 11, 16, 12, 30, 0, DateTimeKind.Utc);
        offer.MarkObserved(observedAt);
        check(offer.ObservedAtUtc == observedAt,
            "A hand-made offer records the date its price was observed");

        var rejected = false;
        try
        {
            offer.MarkObserved(default);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        check(rejected && offer.ObservedAtUtc == observedAt,
            "An empty observation date is rejected and leaves the stored one untouched");

        return Task.CompletedTask;
    }

    private static void Assign(Category category, Product product)
    {
        var link = ProductCategory.Create(product.ProductId, category.CategoryId);
        Set(link, nameof(ProductCategory.Product), product);
        Set(link, nameof(ProductCategory.Category), category);
        ((ICollection<ProductCategory>)category.ProductCategories).Add(link);
    }

    private static void AddPoint(InspectionContext fake, Inventory offer, int price, DateTime observedAtUtc)
    {
        var point = InventoryPriceHistory.Create(offer.InventoryId, price, offer.OriginalPrice,
            offer.DiscountPercentage, offer.Availability, observedAtUtc);
        Set(point, nameof(InventoryPriceHistory.Inventory), offer);
        fake.Histories.Add(point);
    }

    private static void Set(object target, string property, object? value) =>
        target.GetType().GetProperty(property)!.SetValue(target, value);
}
