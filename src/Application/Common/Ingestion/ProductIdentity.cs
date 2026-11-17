using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Entities;

namespace Application.Common.Ingestion;

// Resultado de emparejar una ficha de tienda con el catálogo. Create significa que no hay nada parecido y el
// llamador debe crear producto; Reuse que hay exactamente un producto y hay que actualizar su oferta; Park que una
// persona tiene que decidir y no se publica nada.
public enum ProductIdentityOutcome
{
    Create = 0,
    Reuse = 1,
    Park = 2
}

// Lo que la ficha declara sobre su identidad, ya recortado. El SKU es el del conector de la tienda y el código de
// barras es el GTIN del fabricante cuando la ficha lo informa.
public sealed record ProductIdentityInput(string? Title, string? Vendor, string? Sku, string? Barcode);

// Un producto del catálogo con lo mínimo para emparejar sin arrastrar el grafo de navegación.
public sealed record ProductIdentityCandidate(
    Guid ProductId,
    string ProductName,
    string ManufacturerName,
    string? PartNumber,
    string? Gtin);

public sealed record ProductIdentityDecision(ProductIdentityOutcome Outcome, Guid? ProductId, string? Reason)
{
    public static ProductIdentityDecision Create() => new(ProductIdentityOutcome.Create, null, null);

    public static ProductIdentityDecision Reuse(Guid productId) =>
        new(ProductIdentityOutcome.Reuse, productId, null);

    public static ProductIdentityDecision Park(string reason) => new(ProductIdentityOutcome.Park, null, reason);
}

public sealed record FranchiseCandidate(Guid FranchiseId, string FranchiseName);

/// <summary>
/// Regla de identidad del catálogo aprobada para la ingesta: primero el SKU de la tienda, después el código de
/// barras y por último el título normalizado junto al fabricante. Vive aparte de la base de datos y de la red para
/// que la decisión se pueda comprobar sin infraestructura, y nunca crea ni publica: solo decide.
/// </summary>
public static class ProductIdentity
{
    // Franquicia compartida que se crea cuando el título y el vendedor no permiten reconocer ninguna conocida. Es una
    // sola fila para todo el catálogo ingestado: multiplicarla por tienda volvería a inventar clasificación.
    public const string UnclassifiedFranchiseName = "Sin clasificar";

    public const string MissingTitleReason = "La ficha no informa título.";
    public const string MissingVendorReason = "La ficha no informa vendedor.";
    public const string LongTitleReason = "El título supera los 200 caracteres del catálogo.";
    public const string LongVendorReason = "El vendedor supera los 50 caracteres del catálogo.";
    public const string MissingPriceReason = "La ficha no informa un precio válido.";
    public const string AmbiguousSkuReason = "El SKU coincide con varios productos.";
    public const string AmbiguousBarcodeReason = "El código de barras coincide con varios productos.";
    public const string AmbiguousTitleReason = "El título y el fabricante coinciden con varios productos.";
    public const string ConflictingReferencesReason = "El SKU, el código de barras y el título apuntan a productos distintos.";
    public const string ConflictingSkuReason = "El producto existente informa otro SKU.";
    public const string ConflictingBarcodeReason = "El producto existente informa otro código de barras.";
    public const string NameTakenReason = "El nombre del producto ya existe con otro fabricante.";

    public const int MaximumProductNameLength = 200;
    public const int MaximumManufacturerNameLength = 50;

    // Grados que publican los títulos reales. Las siglas compuestas van primero para que "HGUC" no se lea como "HG"
    // recortado; los límites impiden leer siglas dentro de una palabra ("HGUNDAM" no es High Grade).
    private static readonly Regex GradeToken = new(
        @"(?<![a-z0-9])(hguc|hgce|mgex|hg|mg|rg|pg|sd)(?![a-z0-9])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    // Escalas que el catálogo modela. Se aceptan las dos separaciones que usan las tiendas ("1/144" y "1:144") pero
    // solo esas cuatro relaciones: una escala desconocida se deja en null en vez de inventar una cadena.
    private static readonly Regex ScaleToken = new(
        @"(?<![0-9/])(1\s*[/:]\s*(144|100|60|48))(?![0-9])",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Normaliza un texto para compararlo: minúsculas, sin tildes, sin puntuación y con espacios simples. Dos títulos
    /// que solo difieren en mayúsculas, tildes o guiones son el mismo título para el catálogo.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var pendingSpace = false;
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(character))
            {
                if (pendingSpace && builder.Length > 0) builder.Append(' ');
                pendingSpace = false;
                builder.Append(char.ToLowerInvariant(character));
            }
            else
            {
                pendingSpace = true;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Grado declarado en el título, o <see cref="GunplaGrade.None" /> cuando el título no lo dice. Un título como
    /// "[Preventa] GUNDAM EX Gundam Universe" no declara grado y no se le asigna ninguno.
    /// </summary>
    public static GunplaGrade ParseGrade(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return GunplaGrade.None;
        var match = GradeToken.Match(title);
        if (!match.Success) return GunplaGrade.None;
        return match.Value.ToLowerInvariant() switch
        {
            "hg" or "hguc" or "hgce" => GunplaGrade.HighGrade,
            "mg" or "mgex" => GunplaGrade.MasterGrade,
            "rg" => GunplaGrade.RealGrade,
            "pg" => GunplaGrade.PerfectGrade,
            "sd" => GunplaGrade.SuperDeformed,
            _ => GunplaGrade.None
        };
    }

    /// <summary>
    /// Escala declarada en el título ("1/144", "1/100", "1/60", "1/48"), o null cuando no la declara.
    /// </summary>
    public static string? ParseScale(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        var match = ScaleToken.Match(title);
        if (!match.Success) return null;
        var scale = "1/" + match.Groups[2].Value;
        return GunplaGradeRules.IsValidScale(scale) ? scale : null;
    }

    /// <summary>
    /// Decide si la ficha reutiliza un producto, crea uno nuevo o se aparca. Las referencias se evalúan todas: dos
    /// referencias que apuntan a productos distintos son una contradicción, no un desempate silencioso.
    /// </summary>
    public static ProductIdentityDecision Decide(
        ProductIdentityInput input, IReadOnlyList<ProductIdentityCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(candidates);

        var title = input.Title?.Trim();
        var vendor = input.Vendor?.Trim();
        if (string.IsNullOrEmpty(title)) return ProductIdentityDecision.Park(MissingTitleReason);
        if (string.IsNullOrEmpty(vendor)) return ProductIdentityDecision.Park(MissingVendorReason);
        if (title.Length > MaximumProductNameLength) return ProductIdentityDecision.Park(LongTitleReason);
        if (vendor.Length > MaximumManufacturerNameLength) return ProductIdentityDecision.Park(LongVendorReason);

        var sku = Clean(input.Sku);
        var barcode = Clean(input.Barcode);
        var normalizedTitle = Normalize(title);
        var normalizedVendor = Normalize(vendor);

        var bySku = sku is null
            ? []
            : candidates.Where(candidate => SameText(candidate.PartNumber, sku)).ToArray();
        var byBarcode = barcode is null
            ? []
            : candidates.Where(candidate => SameText(candidate.Gtin, barcode)).ToArray();
        var byTitle = candidates.Where(candidate =>
                Normalize(candidate.ProductName) == normalizedTitle &&
                Normalize(candidate.ManufacturerName) == normalizedVendor)
            .ToArray();

        if (bySku.Length > 1) return ProductIdentityDecision.Park(AmbiguousSkuReason);
        if (byBarcode.Length > 1) return ProductIdentityDecision.Park(AmbiguousBarcodeReason);
        if (byTitle.Length > 1) return ProductIdentityDecision.Park(AmbiguousTitleReason);

        var found = bySku.Concat(byBarcode).Concat(byTitle)
            .GroupBy(candidate => candidate.ProductId)
            .ToArray();
        if (found.Length > 1) return ProductIdentityDecision.Park(ConflictingReferencesReason);
        if (found.Length == 0)
        {
            // El nombre del producto es único en la base. Si ya existe con otro fabricante, crear otra fila fallaría al
            // guardar: se aparca para que una persona decida si es el mismo producto o un homónimo.
            var taken = candidates.Any(candidate => Normalize(candidate.ProductName) == normalizedTitle);
            return taken
                ? ProductIdentityDecision.Park(NameTakenReason)
                : ProductIdentityDecision.Create();
        }

        var match = found[0].First();
        if (sku is not null && match.PartNumber is not null && !SameText(match.PartNumber, sku))
            return ProductIdentityDecision.Park(ConflictingSkuReason);
        if (barcode is not null && match.Gtin is not null && !SameText(match.Gtin, barcode))
            return ProductIdentityDecision.Park(ConflictingBarcodeReason);

        return ProductIdentityDecision.Reuse(match.ProductId);
    }

    /// <summary>
    /// Franquicia conocida que el título o el vendedor nombran. Si dos la nombran gana la más específica (el nombre
    /// más largo) para que "Gundam Unicorn" no se quede en "Gundam"; sin coincidencias devuelve null.
    /// </summary>
    public static FranchiseCandidate? MatchFranchise(
        string? title, string? vendor, IReadOnlyList<FranchiseCandidate> franchises)
    {
        ArgumentNullException.ThrowIfNull(franchises);
        var haystack = Normalize(title) + " " + Normalize(vendor);
        FranchiseCandidate? best = null;
        var bestLength = 0;
        foreach (var franchise in franchises)
        {
            var needle = Normalize(franchise.FranchiseName);
            if (needle.Length == 0 || needle.Length <= bestLength) continue;
            if (!ContainsWholeWords(haystack, needle)) continue;
            best = franchise;
            bestLength = needle.Length;
        }

        return best;
    }

    // El nombre de la franquicia tiene que aparecer como palabras completas: "Gundam" no puede casar con "Gundamx".
    private static bool ContainsWholeWords(string haystack, string needle) =>
        (" " + haystack + " ").Contains(" " + needle + " ", StringComparison.Ordinal);

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    // Los identificadores se comparan sin distinguir mayúsculas porque la columna de SQL Server usa una intercalación
    // que tampoco las distingue: la regla y la base tienen que decidir lo mismo.
    private static bool SameText(string? left, string right) =>
        left is not null && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
