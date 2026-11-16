using System.Reflection;
using Application.Common.Interfaces;
using Application.Dashboard.Queries.GetProductDetail;
using Application.Dashboard.Queries.GetProductPriceHistory;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Services;

internal static class ProductDetailChecks
{
    public static async Task Run(Action<bool, string> check)
    {
        SiteConfig.FolderImage = "images";
        var context = DispatchProxy.Create<IApplicationDbContext, InspectionContext>();
        var fake = (InspectionContext)(object)context;

        var manufacturer = Manufacturer.Create("Bandai");
        var franchise = Franchise.Create("Gundam");
        var building = Category.Create("Maquetas", "bi bi-tools", 1, "maquetas");
        var product = Product.Create("RX-78-2 Gundam", manufacturer.ManufacturerId, franchise.FranchiseId, null, true,
            "8+", "18cm", "Maqueta a escala", new DateOnly(2024, 1, 15), GunplaGrade.HighGrade, "1/144", "HGUC-191");
        var other = Product.Create("Otro producto", manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "15+", "10cm", "Otra maqueta", new DateOnly(2024, 2, 20));
        Set(product, nameof(Product.Manufacturer), manufacturer);
        Set(product, nameof(Product.Franchise), franchise);
        var link = product.AssignCategory(building.CategoryId);
        Set(link, nameof(ProductCategory.Category), building);
        Set(product, nameof(Product.ProductCategories), new List<ProductCategory> { link });
        // Deliberadamente desordenadas: la proyección ordenada debe ordenar por ProductImageOrder.
        Set(product, nameof(Product.ProductImages),
            new List<ProductImage> { product.AssignImage(2), product.AssignImage(1) });
        fake.Products.Add(product);
        fake.Products.Add(other);

        var cheap = Store.Create("Tienda Barata", "https://barata.cl", "bi bi-shop", 1, "barata");
        var middle = Store.Create("Tienda Media", "https://media.cl", "bi bi-shop", 2, "media");
        var expensive = Store.Create("Tienda Cara", "https://cara.cl", "bi bi-shop", 3, "cara");
        var disabled = Store.Create("Tienda Cerrada", "https://cerrada.cl", "bi bi-shop", 4, "cerrada");
        disabled.ToggleActive();
        foreach (var store in new[] { cheap, middle, expensive, disabled }) fake.Stores.Add(store);

        var expensiveOffer = Offer(product, expensive, 25000, 30000, 17);
        var cheapOffer = Offer(product, cheap, 19990, 24990, 20, ProductCondition.Refurbished, "Tarjeta CMR");
        var middleOffer = Offer(product, middle, 21990, 26990, 19);
        var hiddenByAvailability = Offer(product, middle, 15000, 20000, 25);
        Set(hiddenByAvailability, nameof(Inventory.Availability), (bool?)false);
        var hiddenByStore = Offer(product, disabled, 9000, 12000, 25);
        var hiddenByOfferFlag = Offer(product, expensive, 8000, 10000, 20);
        typeof(Inventory).GetProperty(nameof(Inventory.IsActive))!.SetValue(hiddenByOfferFlag, false);
        var otherProductOffer = Offer(other, cheap, 5000, 6000, 17);
        foreach (var offer in new[] { expensiveOffer, cheapOffer, middleOffer, hiddenByAvailability, hiddenByStore,
                     hiddenByOfferFlag, otherProductOffer })
            fake.Rows.Add(offer);

        var detail = new GetProductDetailHandler(context, new ApiResponseService());
        var result = await detail.Handle(new(product.ProductId), default);
        var data = result.Data;
        check(result.Success && result.Message is null && data is not null &&
            data.ProductId == product.ProductId && data.ProductName == "RX-78-2 Gundam" &&
            data.ProductDescription == "Maqueta a escala" && data.ManufacturerName == "Bandai" &&
            data.FranchiseName == "Gundam" && data.SerieName is null && data.ProductHasBase &&
            data.ProductTargetAge == "8+" && data.ProductSize == "18cm" &&
            data.ProductReleaseDate == new DateOnly(2024, 1, 15) && data.Categories.SequenceEqual(new[] { "Maquetas" }),
            "The product page returns the product's own characteristics");

        check(data is not null && data.Grade == (int)GunplaGrade.HighGrade && data.Scale == "1/144" &&
            data.PartNumber == "HGUC-191",
            "The product page publishes the grade, the scale and the part number");

        check(data is not null && data.ImageUrls.Count == 2 &&
            data.ImageUrls[0].StartsWith("/images/", StringComparison.Ordinal) &&
            data.ImageUrls[0].EndsWith(".jpg", StringComparison.Ordinal) &&
            data.ImageUrls[0] != data.ImageUrls[1],
            "Image URLs follow the configured folder and the image order");

        check(data is not null && data.Offers.Select(o => o.Price).SequenceEqual(new[] { 19990, 21990, 25000 }),
            "Offers are ordered from the cheapest store to the most expensive one");
        check(data is not null && data.MinPrice == 19990 && data.MaxPrice == 25000 && data.StoreCount == 3 &&
            data.Offers.Select(o => o.StoreName).SequenceEqual(new[] { "Tienda Barata", "Tienda Media", "Tienda Cara" }),
            "Price range and store count describe only the listed offers");
        check(data is not null && data.Offers.All(o =>
                o.StoreId != disabled.StoreId && o.Availability != false && o.InventoryId != hiddenByOfferFlag.InventoryId &&
                o.InventoryId != otherProductOffer.InventoryId),
            "Closed stores, unavailable offers, disabled offers and other products stay out of the list");
        check(data is not null && data.Offers[0].InventoryId == cheapOffer.InventoryId &&
            data.Offers[0].StoreSlug == "barata" && data.Offers[0].StoreIcon == "bi bi-shop" &&
            data.Offers[0].OriginalPrice == 24990 && data.Offers[0].DiscountPercentage == 20,
            "Each offer carries the store identity and the offer's own price tuple");
        check(data is not null && data.Offers[0].Condition == (int)ProductCondition.Refurbished &&
            data.Offers[0].PaymentMethodLabel == "Tarjeta CMR" &&
            data.Offers[1].Condition == (int)ProductCondition.New && data.Offers[1].PaymentMethodLabel is null,
            "Each offer publishes its integer condition and its payment method without touching the order");

        result = await detail.Handle(new(Guid.NewGuid()), default);
        check(!result.Success && result.Message == "El producto no existe o no está disponible." && result.Data is null,
            "An unknown product fails with a typed envelope instead of an exception");

        // Serie de precios: un punto por día, agregado entre todas las ofertas, del más antiguo al más nuevo.
        var observed = new DateTime(2026, 3, 1, 8, 30, 0, DateTimeKind.Utc);
        AddPoint(fake, cheapOffer, 20000, observed);
        AddPoint(fake, middleOffer, 23000, observed.AddHours(3));
        AddPoint(fake, cheapOffer, 19990, observed.AddDays(1));
        AddPoint(fake, middleOffer, 22500, observed.AddDays(1).AddHours(2));
        AddPoint(fake, otherProductOffer, 4000, observed);

        var history = new GetProductPriceHistoryHandler(context, new ApiResponseService());
        var series = await history.Handle(new(product.ProductId), default);
        var points = series.Data?.Points ?? [];
        check(series.Success && series.Data is not null && series.Data.ProductId == product.ProductId &&
            series.Data.ProductName == "RX-78-2 Gundam" && series.Data.HasHistory && points.Count == 2,
            "The series returns one aggregated point per observed day");
        check(points.Count == 2 && points[0].Date == observed.Date && points[0].MinPrice == 20000 &&
            points[0].MaxPrice == 23000 && points[0].OfferCount == 2 &&
            points[1].MinPrice == 19990 && points[1].MaxPrice == 22500 && points[1].OfferCount == 2 &&
            points[0].Date < points[1].Date,
            "Each point carries the cheapest and dearest observed price and counts only this product's offers");

        // Una prueba de ventana necesita puntos relativos a ahora: las fechas fijas se saldrían de la ventana.
        var windowed = Product.Create("Con ventana", manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "8+", "5cm", "Con dos observaciones", new DateOnly(2024, 4, 1));
        Set(windowed, nameof(Product.Manufacturer), manufacturer);
        Set(windowed, nameof(Product.Franchise), franchise);
        Set(windowed, nameof(Product.ProductCategories), new List<ProductCategory>());
        Set(windowed, nameof(Product.ProductImages), new List<ProductImage>());
        fake.Products.Add(windowed);
        var windowedOffer = Offer(windowed, cheap, 30000, 35000, 14);
        fake.Rows.Add(windowedOffer);
        var recentInstant = DateTime.UtcNow.AddHours(-1);
        AddPoint(fake, windowedOffer, 31000, DateTime.UtcNow.AddDays(-10));
        AddPoint(fake, windowedOffer, 29000, recentInstant);

        var recent = await history.Handle(new(windowed.ProductId, 5), default);
        check(recent.Success && recent.Data is not null && recent.Data.Points.Count == 1 &&
            recent.Data.Points[0].MinPrice == 29000 && recent.Data.Points[0].Date == recentInstant.Date,
            "A positive day window keeps only the observations inside it");
        var wholeSeries = await history.Handle(new(windowed.ProductId), default);
        check(wholeSeries.Success && wholeSeries.Data is not null && wholeSeries.Data.Points.Count == 2,
            "An omitted or zero window keeps the whole recorded series");

        var unknownSeries = await history.Handle(new(Guid.NewGuid()), default);
        check(!unknownSeries.Success &&
            unknownSeries.Message == "El producto no existe o no está disponible." && unknownSeries.Data is null,
            "An unknown product has no price series and fails with a typed envelope");

        var undated = Product.Create("Sin historial", manufacturer.ManufacturerId, franchise.FranchiseId, null,
            false, "8+", "5cm", "Sin observaciones", new DateOnly(2024, 3, 1));
        Set(undated, nameof(Product.Manufacturer), manufacturer);
        Set(undated, nameof(Product.Franchise), franchise);
        Set(undated, nameof(Product.ProductCategories), new List<ProductCategory>());
        Set(undated, nameof(Product.ProductImages), new List<ProductImage>());
        fake.Products.Add(undated);
        var empty = await history.Handle(new(undated.ProductId), default);
        check(empty.Success && empty.Data is not null && !empty.Data.HasHistory && empty.Data.Points.Count == 0,
            "A product without observations reports an empty series instead of a failure");

        var emptyDetail = await detail.Handle(new(undated.ProductId), default);
        check(emptyDetail.Success && emptyDetail.Data is not null && emptyDetail.Data.Offers.Count == 0 &&
            emptyDetail.Data.StoreCount == 0 && emptyDetail.Data.MinPrice == 0 && emptyDetail.Data.MaxPrice == 0,
            "A product nobody sells returns zeroed aggregates and an empty offer list");

        // Desempate: los precios iguales se ordenan por nombre de tienda para que la lista sea estable.
        var tieProduct = NewProduct("Empate", manufacturer, franchise);
        fake.Products.Add(tieProduct);
        var zeta = Store.Create("Zeta", "https://zeta.cl", "bi bi-shop", 10, "zeta");
        var alfa = Store.Create("Alfa", "https://alfa.cl", "bi bi-shop", 11, "alfa");
        fake.Stores.Add(zeta);
        fake.Stores.Add(alfa);
        fake.Rows.Add(Offer(tieProduct, zeta, 20000, 25000, 20));
        fake.Rows.Add(Offer(tieProduct, alfa, 20000, 25000, 20));
        var tie = await detail.Handle(new(tieProduct.ProductId), default);
        check(tie.Data is not null &&
            tie.Data.Offers.Select(o => o.StoreName).SequenceEqual(new[] { "Alfa", "Zeta" }),
            "Equal prices keep a stable order by store name");

        // La disponibilidad es opcional en las ofertas importadas: solo se excluye un false explícito.
        check(tie.Data is not null && tie.Data.Offers.All(o => o.Availability is null) && tie.Data.Offers.Count == 2,
            "An offer without availability information is still listed");

        // El cliente puede pedir una ventana inutilizable: debe acotarla en lugar de lanzar una excepción u ocultar la serie.
        var clamped = NewProduct("Ventana", manufacturer, franchise);
        fake.Products.Add(clamped);
        var clampedOffer = Offer(clamped, cheap, 40000, 45000, 11);
        fake.Rows.Add(clampedOffer);
        var now = DateTime.UtcNow;
        AddPoint(fake, clampedOffer, 41000, now.AddDays(-3700));
        AddPoint(fake, clampedOffer, 40500, now.AddDays(-3600));
        AddPoint(fake, clampedOffer, 39990, now.AddDays(-1));
        var negativeWindow = await history.Handle(new(clamped.ProductId, -5), default);
        check(negativeWindow.Success && negativeWindow.Data is not null &&
            negativeWindow.Data.Points.Count == 3,
            "A negative window clamps to the whole series instead of failing");
        var overflowingWindow = await history.Handle(new(clamped.ProductId, int.MaxValue), default);
        check(overflowingWindow.Success && overflowingWindow.Data is not null &&
            overflowingWindow.Data.Points.Count == 2,
            "A window beyond the maximum clamps without overflow and drops older points");
        var boundaryWindow = await history.Handle(new(clamped.ProductId, 3650), default);
        check(boundaryWindow.Success && boundaryWindow.Data is not null &&
            boundaryWindow.Data.Points.Count == 2,
            "The maximum window keeps only the points inside it");

        // Un producto inactivo no es un 500: ambos endpoints responden con el mismo fallo tipado.
        var removed = NewProduct("Retirado", manufacturer, franchise);
        removed.ToggleActive();
        fake.Products.Add(removed);
        var removedDetail = await detail.Handle(new(removed.ProductId), default);
        var removedSeries = await history.Handle(new(removed.ProductId), default);
        check(!removedDetail.Success && !removedSeries.Success && removedDetail.Data is null &&
            removedSeries.Data is null &&
            removedDetail.Message == "El producto no existe o no está disponible." &&
            removedSeries.Message == removedDetail.Message,
            "An inactive product fails both endpoints with the same typed envelope");
    }

    private static Product NewProduct(string name, Manufacturer manufacturer, Franchise franchise)
    {
        var product = Product.Create(name, manufacturer.ManufacturerId, franchise.FranchiseId, null, false,
            "8+", "5cm", name, new DateOnly(2024, 5, 1));
        Set(product, nameof(Product.Manufacturer), manufacturer);
        Set(product, nameof(Product.Franchise), franchise);
        Set(product, nameof(Product.ProductCategories), new List<ProductCategory>());
        Set(product, nameof(Product.ProductImages), new List<ProductImage>());
        return product;
    }

    private static Inventory Offer(Product product, Store store, int price, int originalPrice, int discount,
        ProductCondition condition = ProductCondition.New, string? paymentMethodLabel = null)
    {
        var offer = Inventory.Create(product.ProductId, store.StoreId, price, originalPrice, discount, condition,
            paymentMethodLabel);
        Set(offer, nameof(Inventory.Product), product);
        Set(offer, nameof(Inventory.Store), store);
        return offer;
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
