using Application.Common.Ingestion;

// Clasificación de la ingesta en las categorías del sitio (T2d). Las reglas son puras: se comprueban sin red y sin
// base de datos, con el vocabulario exacto que la ingesta escribe. La lista final reproduce un corte real del
// catálogo local para dejar a la vista qué harían las reglas con él.
internal static class ClassificationChecks
{
    public static void Run(Action<bool, string> check)
    {
        Vocabulary(check);
        GunplaLandsInMaquetas(check);
        MangaVolumeLandsInMangas(check);
        BoardGameLandsInJuegosDeMesa(check);
        UnrecognizableTitleFallsBack(check);
        Precedence(check);
        MultipleCategoriesOnlyWhenTheRulesSaySo(check);
        DeterminismAndNormalization(check);
        DryRunOverRealCatalog(check);
    }

    // El vocabulario es fijo y en español: cuatro categorías que ya existían con su nombre, icono y orden, y tres
    // declaradas por estas reglas. Cambiarlo cambia la navegación del sitio, así que se fija aquí.
    private static void Vocabulary(Action<bool, string> check)
    {
        var expected = new[]
        {
            ("figuras-de-accion", "Figuras de Acción", "bi bi-person-standing", 1, false),
            ("maquetas", "Maquetas", "bi bi-tools", 2, false),
            ("coleccionables", "Coleccionables", "bi bi-gem", 3, true),
            ("lego", "LEGO", "bi bi-bricks", 4, false),
            ("mangas", "Mangas", "bi bi-book", 5, false),
            ("juegos-de-mesa", "Juegos de Mesa", "bi bi-dice-5", 6, false),
            ("libros", "Libros", "bi bi-journal-text", 7, false)
        };
        check(ProductClassification.Categories.Count == expected.Length,
            "The declared category vocabulary has exactly seven categories");
        for (var index = 0; index < expected.Length && index < ProductClassification.Categories.Count; index++)
        {
            var declared = ProductClassification.Categories[index];
            var (slug, name, icon, order, generic) = expected[index];
            check(declared.Slug == slug && declared.Name == name && declared.Icon == icon &&
                declared.Order == order && declared.IsGeneric == generic,
                "Declared category keeps its stable slug, name, icon, order and specificity: " + slug);
        }

        check(ProductClassification.Categories.Select(category => category.Slug).Distinct().Count() ==
            ProductClassification.Categories.Count &&
            ProductClassification.Categories.Select(category => category.Name).Distinct().Count() ==
            ProductClassification.Categories.Count &&
            ProductClassification.Categories.Select(category => category.Order).Distinct().Count() ==
            ProductClassification.Categories.Count,
            "Slugs, names and orders are unique: the database's unique indexes cannot reject the declared set");
        check(ProductClassification.Fallback.Slug == "coleccionables" && ProductClassification.Fallback.IsGeneric &&
            ProductClassification.Categories.Contains(ProductClassification.Fallback),
            "The fallback is a declared category, not a new one invented per title");
    }

    // (a) Un título de Gunpla aterriza en maquetas, venga la señal del tipo de la tienda, de sus etiquetas o del
    // título: el tipo es el nivel más autoritativo y el título el último recurso.
    private static void GunplaLandsInMaquetas(Action<bool, string> check)
    {
        var byTitle = ProductClassification.Classify(
            new ProductClassificationInput(null, [], "RG 1/144 Gundam RX-78-2"));
        check(byTitle.Slugs.SequenceEqual(new[] { "maquetas" }) &&
            byTitle.Evidence == ClassificationEvidence.Title && byTitle.Matches[0].Keyword == "gundam",
            "a) A Gunpla title lands in maquetas through the title rule");

        var byType = ProductClassification.Classify(new ProductClassificationInput("Model Kit", [], "Optimus Prime"));
        check(byType.Slugs.SequenceEqual(new[] { "maquetas" }) &&
            byType.Evidence == ClassificationEvidence.ProductType,
            "a) The store product type classifies a kit without relying on the title");

        var byTag = ProductClassification.Classify(new ProductClassificationInput(null, ["Gunpla"], "RX-78-2"));
        check(byTag.Slugs.SequenceEqual(new[] { "maquetas" }) &&
            byTag.Evidence == ClassificationEvidence.Tags,
            "a) A single store tag classifies a kit when the type is missing");
    }

    // (b) Un tomo de manga cae en mangas gracias al tipo o a la etiqueta que la tienda publica. Solo con el título la
    // regla no puede adivinarlo ("Beastars 6" no dice "manga"), y eso se registra en vez de inventar categorías.
    private static void MangaVolumeLandsInMangas(Action<bool, string> check)
    {
        var byType = ProductClassification.Classify(new ProductClassificationInput("Manga", [], "Beastars 6"));
        check(byType.Slugs.SequenceEqual(new[] { "mangas" }) &&
            byType.Evidence == ClassificationEvidence.ProductType,
            "b) A manga volume lands in mangas through the store product type");

        var byTag = ProductClassification.Classify(new ProductClassificationInput(null, ["Manga"], "Beastars 6"));
        check(byTag.Slugs.SequenceEqual(new[] { "mangas" }) &&
            byTag.Evidence == ClassificationEvidence.Tags,
            "b) A manga volume lands in mangas through the store tag");

        var byTitle = ProductClassification.Classify(
            new ProductClassificationInput(null, [], "One Piece Vol. 3"));
        check(byTitle.Slugs.SequenceEqual(new[] { "mangas" }) &&
            byTitle.Evidence == ClassificationEvidence.Title,
            "b) A manga volume lands in mangas through the title when it declares the volume");

        check(ProductClassification.Classify(new ProductClassificationInput(null, ["Cómic"], "Batman 1")).Slugs
                .SequenceEqual(new[] { "mangas" }),
            "b) The accent-insensitive vocabulary also reads the store's Cómic tag");

        check(ProductClassification.Classify(new ProductClassificationInput(null, [], "Beastars 6")).Slugs
                .SequenceEqual(new[] { "coleccionables" }),
            "b) A bare title without the store taxonomy stays in the fallback instead of inventing a genre");
    }

    // (c) Un juego de mesa cae en juegos-de-mesa por el tipo o por el nombre del juego en el título.
    private static void BoardGameLandsInJuegosDeMesa(Action<bool, string> check)
    {
        var byType = ProductClassification.Classify(
            new ProductClassificationInput("Juegos de Mesa", [], "ADIVINA QUIÉN?"));
        check(byType.Slugs.SequenceEqual(new[] { "juegos-de-mesa" }) &&
            byType.Evidence == ClassificationEvidence.ProductType,
            "c) A board game lands in juegos-de-mesa through the store product type");

        var byTitle = ProductClassification.Classify(
            new ProductClassificationInput(null, [], "MONOPOLY JUNIOR ESPAÑOL"));
        check(byTitle.Slugs.SequenceEqual(new[] { "juegos-de-mesa" }) &&
            byTitle.Evidence == ClassificationEvidence.Title && byTitle.Matches[0].Keyword == "monopoly",
            "c) A board game lands in juegos-de-mesa through the title keyword");

        check(ProductClassification.Classify(
                    new ProductClassificationInput(null, [], "JUEGO PICTIONARY TRADICIONAL | HASBRO")).Slugs
                .SequenceEqual(new[] { "juegos-de-mesa" }),
            "c) The generic word juego in the title also identifies a board game");
    }

    // (d) Un título irreconocible cae en una sola categoría declarada, nunca en una inventada a partir del título.
    private static void UnrecognizableTitleFallsBack(Action<bool, string> check)
    {
        var unknown = ProductClassification.Classify(new ProductClassificationInput(null, [], "MONOS LOCOS"));
        check(unknown.Matches.Count == 1 && unknown.Slugs.SequenceEqual(new[] { "coleccionables" }) &&
            unknown.Evidence == ClassificationEvidence.Fallback && unknown.Matches[0].Keyword is null,
            "d) An unrecognizable title falls back to the single declared coleccionables category");

        foreach (var input in new[]
                 {
                     new ProductClassificationInput(null, [], ""),
                     new ProductClassificationInput(null, null, null),
                     new ProductClassificationInput("Otra cosa", ["Etiqueta rara"], "Producto sin pistas"),
                     new ProductClassificationInput(null, [], "FISHER PRICE PROYECTOR HIPO HORA DE DORMIR")
                 })
        {
            var decision = ProductClassification.Classify(input);
            check(decision.Matches.Count == 1 && ProductClassification.Categories.Any(category =>
                    category.Slug == decision.Matches[0].Category.Slug),
                "d) Every unrecognizable input yields exactly one declared category: " + (input.Title ?? "null"));
        }
    }

    private static void Precedence(Action<bool, string> check)
    {
        var typeOverTitle = ProductClassification.Classify(
            new ProductClassificationInput("Figura de Acción", [], "Gundam RX-78-2"));
        check(typeOverTitle.Slugs.SequenceEqual(new[] { "figuras-de-accion" }) &&
            typeOverTitle.Evidence == ClassificationEvidence.ProductType,
            "The store product type wins over the title");

        var tagsOverTitle = ProductClassification.Classify(
            new ProductClassificationInput(null, ["Manga"], "Gundam RX-78-2"));
        check(tagsOverTitle.Slugs.SequenceEqual(new[] { "mangas" }),
            "The store tags win over the title");

        var genericTypeLosesToTitle = ProductClassification.Classify(
            new ProductClassificationInput("Juguete", [], "HASBRO GAMING MONOPOLY CHILE"));
        check(genericTypeLosesToTitle.Slugs.SequenceEqual(new[] { "juegos-de-mesa" }) &&
            genericTypeLosesToTitle.Evidence == ClassificationEvidence.Title,
            "A generic store type (Juguete) never hides the specific signal of a lower tier");

        var onlyGeneric = ProductClassification.Classify(new ProductClassificationInput("Juguete", [], "SIN PISTAS"));
        check(onlyGeneric.Slugs.SequenceEqual(new[] { "coleccionables" }) &&
            onlyGeneric.Evidence == ClassificationEvidence.ProductType,
            "A generic type alone still classifies, as coleccionables, instead of falling silent");
    }

    // Varias categorías solo cuando la misma ficha declara varias señales concretas: así una etiqueta genérica no
    // mete todo el catálogo en coleccionables y una señal de otro nivel nunca se suma.
    private static void MultipleCategoriesOnlyWhenTheRulesSaySo(Action<bool, string> check)
    {
        var legoKit = ProductClassification.Classify(
            new ProductClassificationInput(null, ["LEGO", "Model Kit"], "Set espacial"));
        check(legoKit.Slugs.SequenceEqual(new[] { "maquetas", "lego" }) &&
            legoKit.Evidence == ClassificationEvidence.Tags,
            "Two concrete tags produce exactly the two categories they declare");

        var genericSuppressed = ProductClassification.Classify(
            new ProductClassificationInput(null, ["Coleccionable", "Manga"], "Beastars 6"));
        check(genericSuppressed.Slugs.SequenceEqual(new[] { "mangas" }),
            "A generic tag is suppressed when the same sheet declares a concrete category");

        var tagTierStops = ProductClassification.Classify(
            new ProductClassificationInput(null, ["LEGO"], "Gundam RX-78-2"));
        check(tagTierStops.Slugs.SequenceEqual(new[] { "lego" }) &&
            tagTierStops.Evidence == ClassificationEvidence.Tags,
            "Signals only combine inside the winning tier, never across tiers");
    }

    private static void DeterminismAndNormalization(Action<bool, string> check)
    {
        var input = new ProductClassificationInput("Juegos de Mesa", ["Hasbro"], "MONOPOLY JUNIOR ESPAÑOL");
        var first = ProductClassification.Classify(input);
        var second = ProductClassification.Classify(input);
        check(first.Slugs.SequenceEqual(second.Slugs) && first.Evidence == second.Evidence,
            "Classifying the same sheet twice produces the same decision");

        check(ProductClassification.Classify(
                    new ProductClassificationInput(null, ["Manga", "mangas", "MANGA"], "Beastars 6")).Slugs
                .SequenceEqual(new[] { "mangas" }),
            "Repeated tags in any case produce a single category, never duplicated matches");

        check(ProductClassification.Classify(
                    new ProductClassificationInput(null, [], "One Piece Tomo 3")).Slugs
                .SequenceEqual(new[] { "mangas" }) &&
            ProductClassification.Classify(
                    new ProductClassificationInput(null, [], "Atelier 5 Manga")).Slugs
                .SequenceEqual(new[] { "mangas" }),
            "Accents and plurals of the vocabulary are read through the same normalization");

        foreach (var title in new[] { "Gundam", "FISHER PRICE LIBRO ABC PERRITO", "S.H.Figuarts Goku",
            "LEGO Millennium Falcon", "MONOS LOCOS" })
        {
            var decision = ProductClassification.Classify(new ProductClassificationInput(null, [], title));
            check(decision.Slugs.Distinct().Count() == decision.Slugs.Count &&
                decision.Slugs.All(slug => ProductClassification.Categories.Any(c => c.Slug == slug)),
                "Classification only returns unique declared slugs: " + title);
        }
    }

    // Corte del catálogo local (20 productos, 40 ofertas) leído por los end points públicos del dashboard. Se
    // clasifica solo con el título porque las etiquetas y el tipo de producto no se persisten en el catálogo: sin
    // volver a leer la ficha de la tienda no hay más señal, y eso es exactamente lo que muestra esta lista.
    private static void DryRunOverRealCatalog(Action<bool, string> check)
    {
        var catalog = new (string Title, string Expected)[]
        {
            ("Gundam RX-78-2", "maquetas"),
            ("Funko Pop Spider-Man", "figuras-de-accion"),
            ("LEGO Millennium Falcon", "lego"),
            ("S.H.Figuarts Goku", "figuras-de-accion"),
            ("MONOPOLY JUNIOR ESPAÑOL", "juegos-de-mesa"),
            ("FISHER PRICE LLAVERO DE ACTIVIDADES", "coleccionables"),
            ("MARVEL AVENGERS TITAN HERO FIGURA IRON MAN (30CM)", "figuras-de-accion"),
            ("FISHER PRICE PROYECTOR HIPO HORA DE DORMIR", "coleccionables"),
            ("MONOS LOCOS", "coleccionables"),
            ("HASBRO GAMING MONOPOLY CHILE", "juegos-de-mesa"),
            ("Beastars 6", "coleccionables"),
            ("JUEGO PICTIONARY TRADICIONAL | HASBRO", "juegos-de-mesa"),
            ("ADIVINA QUIÉN?", "juegos-de-mesa"),
            ("Beastars 9", "coleccionables"),
            ("Beastars 7", "coleccionables"),
            ("Atelier of the Witch Hat 5", "coleccionables"),
            ("LITTLE MOMMY PREPARANDO GALLETAS", "coleccionables"),
            ("FISHER PRICE LIBRO ABC PERRITO", "libros"),
            ("The Ancient Magus Bride 5", "coleccionables"),
            ("MARVEL ACTION LOS VENGADORES 2. EL RUBÍ EMERSIÓN", "coleccionables")
        };

        var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        Console.WriteLine("Dry classification of the 20 products / 40 offers in the local catalog (title rules only):");
        foreach (var (title, expected) in catalog)
        {
            var decision = ProductClassification.Classify(new ProductClassificationInput(null, [], title));
            var slug = decision.Slugs.Single();
            check(slug == expected, "Dry classification of the real title " + title + " is " + expected);
            counts[slug] = counts.GetValueOrDefault(slug) + 1;
            Console.WriteLine($"  {title} -> {slug} ({decision.Evidence.ToString().ToLowerInvariant()})");
        }

        check(counts.GetValueOrDefault("maquetas") == 1 &&
            counts.GetValueOrDefault("figuras-de-accion") == 3 &&
            counts.GetValueOrDefault("lego") == 1 &&
            counts.GetValueOrDefault("juegos-de-mesa") == 4 &&
            counts.GetValueOrDefault("libros") == 1 &&
            counts.GetValueOrDefault("coleccionables") == 10 &&
            counts.Values.Sum() == 20,
            "The dry classification of the recorded catalog adds up to 20 products with the expected per-category counts");
        Console.WriteLine("  totals: " + string.Join(", ", counts.Select(pair => pair.Key + "=" + pair.Value)));
    }
}
