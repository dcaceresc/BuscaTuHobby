namespace Domain.Entities;

// Condición física de una oferta. Los números son el contrato HTTP con el frontend (0 nueva, 1 refabricada,
// 2 usada) porque la API no configura JsonStringEnumConverter y los envía como enteros.
public enum ProductCondition
{
    New = 0,
    Refurbished = 1,
    Used = 2
}

// Reglas y mensajes compartidos por la entidad y los casos de uso, para que el rechazo sea idéntico en ambos lados.
public static class OfferConditionRules
{
    public const int MaxPaymentMethodLabelLength = 64;

    public const string InvalidConditionMessage =
        "La condición de la oferta debe ser 0 (nueva), 1 (refabricada) o 2 (usada).";

    public const string InvalidPaymentMethodLabelMessage =
        "La etiqueta del medio de pago no puede estar vacía y admite hasta 64 caracteres.";

    public static bool IsValidCondition(int value) =>
        value == (int)ProductCondition.New || value == (int)ProductCondition.Refurbished ||
        value == (int)ProductCondition.Used;

    // La etiqueta es opcional: null significa que la oferta no informa medio de pago.
    public static bool IsValidPaymentMethodLabel(string? label) =>
        label is null || (label.Length is > 0 and <= MaxPaymentMethodLabelLength && label == label.Trim());
}
