using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.PriceHistory;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Domain.Entities;

namespace Infrastructure.Services;

// Convierte los elementos encolados por una ejecución en ofertas y productos. No descubre ni inventa identidad: lee
// cada ficha con el conector de la tienda, aplica la regla de identidad aprobada y aparca lo dudoso. El limitador por
// dominio y el portal de robots viven dentro del conector, así que esta pasada no tiene otra puerta que los esquive.
public sealed class ImportRunProcessor(
    IApplicationDbContext context,
    IEnumerable<IStoreConnector> connectors) : IImportRunProcessor
{
    private const int StoredHttpStatus = 200;
    private const string OfferBoundToAnotherSheetReason =
        "La oferta de la tienda ya está vinculada a otra ficha.";

    private readonly IApplicationDbContext _context = context;
    private readonly IReadOnlyList<IStoreConnector> _connectors = connectors.ToArray();

    public async Task<ImportRunProcessingResult> ProcessAsync(
        StoreContext store, Guid importRunId, DateTime observedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (observedAtUtc == default || observedAtUtc.Kind != DateTimeKind.Utc)
            throw new System.ArgumentException("La fecha de la observación debe ser una fecha UTC.",
                nameof(observedAtUtc));

        var run = await _context.ImportRuns
            .FirstOrDefaultAsync(row => row.ImportRunId == importRunId, cancellationToken)
            ?? throw new NotFoundException($"La ejecución {importRunId} no existe.");
        if (run.StoreId != store.StoreId)
            throw new System.ArgumentException("La ejecución no pertenece a la tienda indicada.", nameof(store));

        // Misma resolución por clave que usa el cargador manual: una tienda nueva es una fila más, no una rama más.
        var connector = _connectors.FirstOrDefault(candidate =>
                string.Equals(candidate.Key, store.ConnectorKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new StoreConfigurationException(
                $"La tienda {store.StoreName} declara un conector desconocido: {store.ConnectorKey}.");

        var items = await _context.ImportItems
            .Where(item => item.ImportRunId == importRunId)
            .ToListAsync(cancellationToken);

        // Reanudación y orden salen de la regla de T2a: pendientes y errores transitorios, nunca un aparcado ni un
        // guardado. Una ejecución completada no se reanuda y la regla ya lo rechaza antes de tocar nada.
        var work = ImportRunPlanner.Resume(run, items);
        var catalog = await LoadCatalogAsync(cancellationToken);
        var budget = new RequestBudget(run.RequestBudget);

        var stored = 0;
        var parked = 0;
        var skipped = 0;
        var failed = 0;
        var handled = 0;
        var budgetExhausted = false;
        var stopped = false;

        foreach (var item in work)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Cada ficha cuesta una petición: el presupuesto de la ejecución es el tope real de la pasada.
            if (!budget.TryConsume())
            {
                budgetExhausted = true;
                stopped = true;
                break;
            }

            StoreProductSheet sheet;
            try
            {
                sheet = await connector.LoadSheetAsync(store, item.SourceUrl, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Cancelar no es un fallo de la ficha: se propaga sin guardar para que la reanudación retome lo que
                // quedó pendiente, y el llamador no recibe una ejecución cerrada a medias.
                throw;
            }
            catch (StorePathDisallowedException exception)
            {
                // robots.txt excluyó la ruta antes de pedirla: se audita como descarte, no como fallo.
                item.MarkSkipped(null, Reason(exception.Message), observedAtUtc);
                run.RecordSkipped(1);
                skipped++;
                handled++;
                continue;
            }
            catch (StoreRateLimitException exception)
            {
                // El conector ya agotó sus reintentos. Insistir con el mismo host es lo contrario de la cortesía que
                // impone el limitador: el elemento queda como fallo transitorio y la pasada termina sin cerrar la ejecución.
                item.MarkFailed(StatusOf(exception), Reason(exception.Message), observedAtUtc);
                run.RecordFailed(1);
                failed++;
                handled++;
                stopped = true;
                break;
            }
            catch (StoreConfigurationException exception)
            {
                // Una tienda mal configurada falla igual en todas las fichas: no tiene sentido gastar el presupuesto.
                item.MarkFailed(null, Reason(exception.Message), observedAtUtc);
                run.RecordFailed(1);
                failed++;
                handled++;
                stopped = true;
                break;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Un fallo de una ficha no puede tumbar la pasada completa: queda auditado y la ejecución sigue.
                item.MarkFailed(StatusOf(exception), Reason(ReasonFor(exception)), observedAtUtc);
                run.RecordFailed(1);
                failed++;
                handled++;
                continue;
            }

            var parkReason = await ParkOrStoreAsync(sheet, store, catalog, observedAtUtc, cancellationToken);
            if (parkReason is null)
            {
                item.MarkStored(StoredHttpStatus, observedAtUtc);
                run.RecordStored(1);
                stored++;
            }
            else
            {
                item.MarkParked(parkReason);
                run.RecordSkipped(1);
                parked++;
            }

            handled++;
        }

        if (!stopped)
        {
            // El fin nunca puede ser anterior al inicio: un reloj desfasado no puede dejar la fila inválida.
            run.Complete(observedAtUtc < run.StartedAtUtc ? run.StartedAtUtc : observedAtUtc);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new ImportRunProcessingResult(stored, parked, skipped, failed, work.Count - handled, !stopped,
            budgetExhausted);
    }

    /// <summary>
    /// Aplica la identidad a una ficha leída. Devuelve null cuando la oferta quedó guardada y el motivo cuando hay que
    /// aparcarla; en ese caso no se crea ni se modifica ningún producto ni ninguna oferta.
    /// </summary>
    private async Task<string?> StoreAsync(StoreProductSheet preview, StoreContext store, Catalog catalog,
        DateTime observedAtUtc, CancellationToken cancellationToken)
    {
        var variant = ChooseVariant(preview);
        if (variant is null) return ProductIdentity.MissingPriceReason;

        var identity = new ProductIdentityInput(preview.Title, preview.Vendor, Optional(variant.Sku), Optional(variant.Barcode));
        var decision = ProductIdentity.Decide(identity, Candidates(catalog));
        if (decision.Outcome == ProductIdentityOutcome.Park)
            return decision.Reason ?? ProductIdentity.MissingTitleReason;

        var isNewProduct = decision.Outcome != ProductIdentityOutcome.Reuse;
        var product = isNewProduct
            ? CreateProduct(identity, preview.DescriptionHtml, catalog)
            : catalog.Products.Single(candidate => candidate.ProductId == decision.ProductId);

        var price = ToWholePrice(variant.Price);
        if (price <= 0) return ProductIdentity.MissingPriceReason;
        var original = variant.OriginalPrice is { } compare && compare > variant.Price
            ? ToWholePrice(compare)
            : 0;
        if (original <= price) original = 0;
        var discount = original <= 0
            ? 0
            : (int)Math.Round((original - price) * 100m / original, MidpointRounding.AwayFromZero);

        var existing = await _context.Inventories.FirstOrDefaultAsync(row =>
            row.StoreId == store.StoreId && row.ProductId == product.ProductId, cancellationToken);

        // Condición y medio de pago: la ficha pública de Shopify no los informa, así que la oferta se registra como
        // nueva y sin medio en vez de inventar un dato que el catálogo no puede sostener.
        if (existing is null)
        {
            // La oferta se construye antes de añadir nada al contexto: si el dominio la rechaza, la ficha se aparca
            // sin dejar un producto sin oferta en el catálogo.
            var offer = Inventory.CreateImportedOffer(product.ProductId, store.StoreId, price, original, discount,
                preview.SourceUrl, variant.Id, variant.Available, observedAtUtc);
            if (isNewProduct)
            {
                _context.Products.Add(product);
                catalog.Products.Add(product);
            }

            _context.Inventories.Add(offer);
            // Una oferta importada nace con su punto base, igual que las que crea el mantenedor: sin él su gráfico de
            // precios quedaría vacío hasta el primer cambio de precio, y una bajada nunca tendría dos puntos que comparar.
            InventoryPriceHistoryRecorder.RecordBaseline(_context, offer);
            // El producto nace ya clasificado: sin categoría no aparece al navegar y queda fuera del catálogo real.
            EnsureClassification(product, preview, catalog);
            return null;
        }

        // El par (tienda, producto) es único, así que una segunda variante del mismo producto no puede ser otra oferta:
        // se aparca en vez de robarle la vinculación a la ficha que ya la tiene.
        if (existing.SourceUrl != preview.SourceUrl || existing.ExternalVariantId != variant.Id)
            return OfferBoundToAnotherSheetReason;

        var before = InventoryPricePoint.Capture(existing);
        existing.UpdateImportedOffer(price, original, discount, preview.SourceUrl, variant.Id, variant.Available,
            observedAtUtc, ProductCondition.New, null);
        // El registrador sólo añade un punto cuando el precio cambió: una segunda pasada idéntica no infla la serie.
        InventoryPriceHistoryRecorder.Record(_context, existing, before);
        // Un producto reutilizado que la ingesta creó antes sin clasificar se completa aquí; el que ya tiene categoría
        // (humana o de otra pasada) no se toca.
        EnsureClassification(product, preview, catalog);
        return null;
    }

    /// <summary>
    /// Escribe la clasificación de un producto que la ingesta acaba de guardar. Nunca quita nada: si el producto ya
    /// tiene alguna categoría se supone que una persona la asignó y la ingesta no la reemplaza. El vínculo
    /// (producto, categoría) se deduplica contra la base y contra la propia pasada para que reejecutar no duplique.
    /// </summary>
    private void EnsureClassification(Product product, StoreProductSheet sheet, Catalog catalog)
    {
        if (!catalog.Classification.ClassifiedProducts.Add(product.ProductId)) return;

        var decision = ProductClassification.Classify(new ProductClassificationInput(
            sheet.DeclaredProductType, sheet.Tags, sheet.Title));
        foreach (var match in decision.Matches)
        {
            var category = CategoryFor(match.Category, catalog);
            if (!catalog.Classification.Links.Add((product.ProductId, category.CategoryId))) continue;
            _context.ProductCategories.Add(ProductCategory.Create(product.ProductId, category.CategoryId));
        }
    }

    // La categoría se resuelve por slug: una fila existente se reutiliza tal como está (nombre, icono y orden los
    // administra una persona) y solo se crea la que las reglas declaran. El nombre es único en la base igual que el
    // slug, así que también se busca por nombre: una persona pudo crearla con otro slug y crear la declarada haría
    // fallar el guardado de toda la pasada. CategoryOrder también es único, así que una categoría nueva toma el primer
    // orden libre desde el declarado en vez de chocar.
    /// <summary>
    /// Clasifica el catálogo existente con lo único que se conserva de una ficha antigua: el título. Sin esto, cada
    /// producto que entró antes de existir la clasificación se quedaría sin categoría, porque su URL nunca se vuelve
    /// a procesar. Solo toca productos sin ninguna categoría: lo que una persona asignó no se reemplaza.
    /// </summary>
    public async Task<int> ClassifyExistingAsync(CancellationToken cancellationToken)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var pending = catalog.Products
            .Where(product => !catalog.Classification.ClassifiedProducts.Contains(product.ProductId))
            .ToList();
        if (pending.Count == 0) return 0;

        foreach (var product in pending)
        {
            var decision = ProductClassification.Classify(
                new ProductClassificationInput(null, [], product.ProductName));
            foreach (var match in decision.Matches)
            {
                var category = CategoryFor(match.Category, catalog);
                if (!catalog.Classification.Links.Add((product.ProductId, category.CategoryId))) continue;
                _context.ProductCategories.Add(ProductCategory.Create(product.ProductId, category.CategoryId));
            }

            // Evita que un segundo pase vuelva a clasificar el mismo producto dentro de la misma ejecución.
            catalog.Classification.ClassifiedProducts.Add(product.ProductId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return pending.Count;
    }

    private Category CategoryFor(DeclaredCategory declared, Catalog catalog)
    {
        var existing = catalog.Categories.FirstOrDefault(category =>
                string.Equals(category.CategorySlug, declared.Slug, StringComparison.OrdinalIgnoreCase)) ??
            catalog.Categories.FirstOrDefault(category =>
                ProductIdentity.Normalize(category.CategoryName) == ProductIdentity.Normalize(declared.Name));
        if (existing is not null) return existing;

        var order = declared.Order;
        while (catalog.Categories.Any(category => category.CategoryOrder == order)) order++;
        var created = Category.Create(declared.Name, declared.Icon, order, declared.Slug);
        _context.Categories.Add(created);
        catalog.Categories.Add(created);
        return created;
    }

    private Product CreateProduct(ProductIdentityInput identity, string? description, Catalog catalog)
    {
        var title = identity.Title!.Trim();
        var vendor = identity.Vendor!.Trim();

        var manufacturer = catalog.Manufacturers.FirstOrDefault(candidate =>
            ProductIdentity.Normalize(candidate.ManufacturerName) == ProductIdentity.Normalize(vendor));
        if (manufacturer is null)
        {
            manufacturer = Manufacturer.Create(vendor);
            _context.Manufacturers.Add(manufacturer);
            catalog.Manufacturers.Add(manufacturer);
        }

        var known = ProductIdentity.MatchFranchise(identity.Title, identity.Vendor,
            catalog.Franchises.Select(row => new FranchiseCandidate(row.FranchiseId, row.FranchiseName)).ToArray());
        var franchiseId = known?.FranchiseId ?? UnclassifiedFranchiseId(catalog);

        // Los datos que la ficha no informa no se inventan: sin base, sin edad y sin tamaño se guardan como el vacío
        // que ya admite el catálogo, y la fecha mínima es el "sin fecha" de una columna que no admite null.
        var product = Product.Create(title, manufacturer.ManufacturerId, franchiseId, null,
            productHasBase: false,
            productTargetAge: string.Empty,
            productSize: string.Empty,
            productDescription: description?.Trim() ?? string.Empty,
            productReleaseDate: DateOnly.MinValue,
            grade: ProductIdentity.ParseGrade(identity.Title),
            scale: ProductIdentity.ParseScale(identity.Title),
            partNumber: identity.Sku,
            gtin: identity.Barcode);
        // El producto se añade al contexto desde StoreAsync, solo cuando la oferta también se pudo construir.
        return product;
    }

    // Un dato que el dominio rechaza invalida esa ficha, nunca la pasada completa: se aparca con el motivo para que
    // una revisión lo corrija en lugar de tumbar la importación entera.
    private async Task<string?> ParkOrStoreAsync(StoreProductSheet preview, StoreContext store, Catalog catalog,
        DateTime observedAtUtc, CancellationToken cancellationToken)
    {
        try
        {
            return await StoreAsync(preview, store, catalog, observedAtUtc, cancellationToken);
        }
        catch (Exception exception) when (exception is System.ArgumentException or InvalidOperationException)
        {
            return Reason(exception.Message);
        }
    }

    // Una ficha puede no informar SKU ni código de barras, y el conector los entrega como cadena vacía: para la
    // identidad "sin dato" y "dato vacío" son lo mismo, y el dominio rechaza un número de parte vacío.
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // "Sin clasificar" es una sola fila compartida, creada a demanda: una franquicia por título inventaría la
    // clasificación que la ingesta justamente no puede deducir.
    private Guid UnclassifiedFranchiseId(Catalog catalog)
    {
        var normalized = ProductIdentity.Normalize(ProductIdentity.UnclassifiedFranchiseName);
        var franchise = catalog.Franchises.FirstOrDefault(row =>
            ProductIdentity.Normalize(row.FranchiseName) == normalized);
        if (franchise is not null) return franchise.FranchiseId;

        franchise = Franchise.Create(ProductIdentity.UnclassifiedFranchiseName);
        _context.Franchises.Add(franchise);
        catalog.Franchises.Add(franchise);
        return franchise.FranchiseId;
    }

    // La primera variante disponible con precio; si ninguna está disponible, la primera con precio. Sin precio válido
    // no hay oferta que publicar y la ficha se aparca.
    private static IrionVariantPreviewDto? ChooseVariant(StoreProductSheet preview)
    {
        var priced = preview.Variants.Where(variant => variant.Price > 0).ToArray();
        return priced.FirstOrDefault(variant => variant.Available) ?? priced.FirstOrDefault();
    }

    // El catálogo se carga una vez por pasada, no por ficha: el emparejamiento por título normalizado no se puede
    // traducir a SQL, así que la comparación se hace en memoria sobre una foto que la propia pasada va ampliando. Las
    // categorías y los vínculos existentes se cargan igual para no duplicar filas ni pisar una clasificación humana.
    private async Task<Catalog> LoadCatalogAsync(CancellationToken cancellationToken)
    {
        var links = await _context.ProductCategories.AsNoTracking().ToListAsync(cancellationToken);
        var classification = new ClassificationState();
        foreach (var link in links)
        {
            classification.ClassifiedProducts.Add(link.ProductId);
            classification.Links.Add((link.ProductId, link.CategoryId));
        }

        return new Catalog(
            await _context.Products.AsNoTracking().ToListAsync(cancellationToken),
            await _context.Manufacturers.AsNoTracking().ToListAsync(cancellationToken),
            await _context.Franchises.AsNoTracking().ToListAsync(cancellationToken),
            await _context.Categories.AsNoTracking().ToListAsync(cancellationToken),
            classification);
    }

    private static IReadOnlyList<ProductIdentityCandidate> Candidates(Catalog catalog) =>
        catalog.Products.Select(product => new ProductIdentityCandidate(
                product.ProductId,
                product.ProductName,
                catalog.Manufacturers
                    .FirstOrDefault(row => row.ManufacturerId == product.ManufacturerId)?.ManufacturerName ??
                string.Empty,
                product.PartNumber,
                product.Gtin))
            .ToArray();

    // CLP se guarda en enteros: el precio de la ficha es decimal y se redondea al peso más cercano, nunca se trunca.
    private static int ToWholePrice(decimal value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    private static int? StatusOf(Exception exception) => exception switch
    {
        StoreRateLimitException => 429,
        HttpRequestException { StatusCode: { } status } => (int)status,
        _ => null
    };

    private static string ReasonFor(Exception exception) => exception switch
    {
        StoreOfferUrlException => "La URL de la ficha no es válida.",
        InvalidDataException => "La ficha no se pudo interpretar.",
        HttpRequestException => "No se pudo leer la ficha de la tienda.",
        _ => "No se pudo procesar la ficha."
    };

    // La columna de error admite 512 caracteres: el motivo se recorta en vez de hacer fallar la auditoría.
    private static string Reason(string message)
    {
        var trimmed = (message ?? string.Empty).Trim();
        if (trimmed.Length == 0) trimmed = "No se pudo procesar la ficha.";
        return trimmed.Length <= ImportItem.MaximumErrorMessageLength
            ? trimmed
            : trimmed[..ImportItem.MaximumErrorMessageLength];
    }

    // La foto del catálogo que la pasada usa para emparejar y amplía al crear.
    private sealed record Catalog(List<Product> Products, List<Manufacturer> Manufacturers, List<Franchise> Franchises,
        List<Category> Categories, ClassificationState Classification);

    // Qué productos ya están clasificados y qué vínculos existen, para no duplicar ProductCategory en una segunda
    // pasada. La pasada los amplía al escribir para que dos fichas del mismo producto no creen dos veces lo mismo.
    private sealed class ClassificationState
    {
        public HashSet<Guid> ClassifiedProducts { get; } = [];
        public HashSet<(Guid ProductId, Guid CategoryId)> Links { get; } = [];
    }
}
