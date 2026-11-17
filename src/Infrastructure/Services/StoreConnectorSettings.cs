using System.Text.Json;
using Application.Common.Exceptions;

namespace Infrastructure.Services;

/// <summary>
/// Ajustes por tienda guardados como JSON en <c>Store.ConnectorSettings</c>. Todo lo que la tienda declare
/// manda sobre el predeterminado; una configuración ilegible falla de forma explícita en vez de inventar moneda
/// o presupuesto.
/// </summary>
public sealed record StoreConnectorSettings(string Currency, int RequestsPerMinute, int RequestBudget)
{
    public const string DefaultCurrency = "CLP";
    public const int DefaultRequestsPerMinute = 20;
    public const int DefaultRequestBudget = 500;

    public static StoreConnectorSettings Parse(string? json, string storeName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new(DefaultCurrency, DefaultRequestsPerMinute, DefaultRequestBudget);

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw new StoreConfigurationException(
                    $"Los ajustes del conector de {storeName} deben ser un objeto JSON.");

            var currency = Text(root, "currency", storeName) ?? DefaultCurrency;
            if (currency.Length != 3 || !currency.All(char.IsLetter))
                throw new StoreConfigurationException(
                    $"La moneda configurada para {storeName} no es un código ISO de tres letras.");

            return new(currency.ToUpperInvariant(),
                Number(root, "requestsPerMinute", DefaultRequestsPerMinute, storeName),
                Number(root, "requestBudget", DefaultRequestBudget, storeName));
        }
        catch (JsonException exception)
        {
            throw new StoreConfigurationException(
                $"Los ajustes del conector de {storeName} no son JSON válido.", exception);
        }
    }

    private static string? Text(JsonElement root, string name, string storeName)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return null;
        if (value.ValueKind != JsonValueKind.String)
            throw new StoreConfigurationException(
                $"El ajuste {name} de {storeName} debe ser texto.");
        return value.GetString()?.Trim();
    }

    private static int Number(JsonElement root, string name, int fallback, string storeName)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return fallback;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number) || number <= 0)
            throw new StoreConfigurationException(
                $"El ajuste {name} de {storeName} debe ser un entero positivo.");
        return number;
    }
}
