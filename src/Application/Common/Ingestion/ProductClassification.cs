namespace Application.Common.Ingestion;

/// <summary>
/// Categoría que las reglas de ingesta declaran. El slug es la clave estable: es lo que la navegación del sitio usa
/// en <c>/categories/:slug</c> y lo que la ingesta busca antes de crear nada.
/// </summary>
/// <param name="IsGeneric">
/// Marca las categorías que son un cajón de sastre ("Coleccionables"). Una coincidencia genérica describe cualquier
/// producto de la tienda, así que no puede tapar una categoría concreta que la misma ficha declare más abajo.
/// </param>
public sealed record DeclaredCategory(string Slug, string Name, string Icon, int Order, bool IsGeneric = false);

// Nivel que decidió la clasificación. Se guarda con la decisión para poder explicarla: "quedó en X" sin decir por qué
// señal no sirve para auditar una ingesta.
public enum ClassificationEvidence
{
    ProductType = 0,
    Tags = 1,
    Title = 2,
    Fallback = 3
}

// Categoría asignada y palabra del vocabulario que la activó. En el caso de reserva no hay palabra: la decisión es la
// ausencia de señales.
public sealed record CategoryMatch(DeclaredCategory Category, string? Keyword);

public sealed record ProductClassificationDecision(
    IReadOnlyList<CategoryMatch> Matches,
    ClassificationEvidence Evidence)
{
    public IReadOnlyList<string> Slugs => Matches.Select(match => match.Category.Slug).ToArray();
}

// Lo que una ficha declara sobre sí misma, ya recortado. El tipo de producto es lo que la tienda dice que vende; las
// etiquetas son sus agrupaciones y el título es la última señal disponible.
public sealed record ProductClassificationInput(
    string? ProductType,
    IReadOnlyList<string>? Tags,
    string? Title);

/// <summary>
/// Reglas deterministas que clasifican una ficha en las categorías del sitio. Viven aparte de la base de datos y de
/// la red para que se puedan comprobar solas. La precedencia es explícita y se evalúa por niveles: tipo de producto,
/// después etiquetas y por último título. El primer nivel con una coincidencia concreta decide; una coincidencia
/// genérica se recuerda y solo se aplica si ningún nivel aporta nada concreto. Si nada coincide, la ficha cae en una
/// única categoría declarada: nunca se inventa una categoría por título.
/// </summary>
public static class ProductClassification
{
    // Orden igual al de la navegación del sitio: las cuatro categorías que ya existían conservan su nombre, icono y
    // orden (el seed y la base local ya las tienen) y las tres nuevas se declaran con nombre, icono y orden fijos.
    public static IReadOnlyList<DeclaredCategory> Categories { get; } =
    [
        new("figuras-de-accion", "Figuras de Acción", "bi bi-person-standing", 1),
        new("maquetas", "Maquetas", "bi bi-tools", 2),
        new("coleccionables", "Coleccionables", "bi bi-gem", 3, IsGeneric: true),
        new("lego", "LEGO", "bi bi-bricks", 4),
        new("mangas", "Mangas", "bi bi-book", 5),
        new("juegos-de-mesa", "Juegos de Mesa", "bi bi-dice-5", 6),
        new("libros", "Libros", "bi bi-journal-text", 7)
    ];

    // El vocabulario de reserva es una categoría que ya existe: crear "Otros" por cada título irreconocible volvería
    // a inventar clasificación, y Coleccionables es el cajón que el sitio ya usa para el hobby sin género claro.
    public static DeclaredCategory Fallback { get; } = Categories.Single(category => category.Slug == "coleccionables");

    // El vocabulario está en español y sin tildes porque se compara con la normalización de ProductIdentity. Las
    // palabras genéricas ("kit", "juego") van después de las compuestas ("model kit", "juego de mesa") para que el
    // motivo registrado sea el más específico que coincidió.
    private static readonly (DeclaredCategory Category, string[] Keywords)[] Rules =
    [
        (Categories[0], ["figura", "figuras", "action figure", "action figures", "figure", "figures",
            "figuarts", "sh figuarts", "banpresto", "nendoroid", "funko", "estatua", "estatuas", "statue"]),
        (Categories[1], ["model kit", "model kits", "maqueta", "maquetas", "gunpla", "gundam", "modelismo",
            "plastimodelo", "modelo a escala", "kit", "kits"]),
        (Categories[2], ["coleccionable", "coleccionables", "collectible", "collectibles", "merchandising",
            "juguete", "juguetes", "toy", "toys"]),
        (Categories[3], ["lego"]),
        (Categories[4], ["manga", "mangas", "tomo", "tomos", "vol", "vols", "volumen", "volumenes", "volume",
            "volumes", "comic", "comics", "historieta", "historietas"]),
        (Categories[5], ["juego de mesa", "juegos de mesa", "board game", "board games", "monopoly", "pictionary",
            "adivina quien", "ajedrez", "jenga", "domino", "rompecabezas", "puzzle", "puzzles", "ludo", "juego",
            "juegos", "cartas"]),
        (Categories[6], ["libro", "libros", "book", "books", "novela", "novelas", "artbook", "art book", "guia",
            "guias", "enciclopedia", "enciclopedias", "manual", "manuales", "revista", "revistas"])
    ];

    public static ProductClassificationDecision Classify(ProductClassificationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var tiers = new (ClassificationEvidence Evidence, IEnumerable<string?> Values)[]
        {
            (ClassificationEvidence.ProductType, [input.ProductType]),
            (ClassificationEvidence.Tags, input.Tags ?? []),
            (ClassificationEvidence.Title, [input.Title])
        };

        CategoryMatch? generic = null;
        var genericEvidence = ClassificationEvidence.Fallback;
        foreach (var (evidence, values) in tiers)
        {
            var matches = Match(values);
            if (matches.Count == 0) continue;

            var specific = matches.Where(match => !match.Category.IsGeneric).ToArray();
            if (specific.Length > 0) return new ProductClassificationDecision(specific, evidence);

            if (generic is null)
            {
                generic = matches[0];
                genericEvidence = evidence;
            }
        }

        // Sin ninguna señal concreta queda la genérica si existió ("Coleccionable", "Juguete") y, si no, la reserva
        // declarada. En ambos casos es una sola categoría y ninguna es nueva.
        return generic is not null
            ? new ProductClassificationDecision([generic], genericEvidence)
            : new ProductClassificationDecision([new CategoryMatch(Fallback, null)],
                ClassificationEvidence.Fallback);
    }

    // Una categoría se activa una sola vez aunque varias etiquetas la nombren: el vínculo del catálogo es único por
    // (producto, categoría) y repetirlo haría fallar el guardado.
    private static List<CategoryMatch> Match(IEnumerable<string?> values)
    {
        var texts = values
            .Select(ProductIdentity.Normalize)
            .Where(text => text.Length > 0)
            .ToArray();
        var matches = new List<CategoryMatch>();
        if (texts.Length == 0) return matches;

        foreach (var (category, keywords) in Rules)
        {
            var keyword = keywords
                .Select(ProductIdentity.Normalize)
                .FirstOrDefault(candidate => texts.Any(text => ContainsWholeWords(text, candidate)));
            if (keyword is not null) matches.Add(new CategoryMatch(category, keyword));
        }

        return matches;
    }

    // La palabra tiene que aparecer completa: "libro" no puede casar dentro de "librote" ni "kit" dentro de "kitchen".
    private static bool ContainsWholeWords(string haystack, string needle) =>
        needle.Length > 0 && (" " + haystack + " ").Contains(" " + needle + " ", StringComparison.Ordinal);
}
