namespace Domain.Entities;

// Grado de la maqueta. Los números son el contrato HTTP con el frontend (0 sin grado, 1 high grade, 2 master
// grade, 3 real grade, 4 perfect grade, 5 super deformed, 6 otro) porque la API no configura
// JsonStringEnumConverter y los envía como enteros.
public enum GunplaGrade
{
    None = 0,
    HighGrade = 1,
    MasterGrade = 2,
    RealGrade = 3,
    PerfectGrade = 4,
    SuperDeformed = 5,
    Other = 6
}

// Reglas y mensajes compartidos por la entidad y los casos de uso, para que el rechazo sea idéntico en ambos lados.
public static class GunplaGradeRules
{
    public const int MaxScaleLength = 16;
    public const int MaxPartNumberLength = 64;

    public const string InvalidGradeMessage =
        "El grado de la maqueta debe ser un valor entre 0 (sin grado) y 6 (otro).";

    public const string InvalidScaleMessage =
        "La escala no puede estar vacía ni tener espacios en los extremos y admite hasta 16 caracteres.";

    public const string InvalidPartNumberMessage =
        "El número de parte no puede estar vacío ni tener espacios en los extremos y admite hasta 64 caracteres.";

    public static bool IsValidGrade(int value) =>
        value >= (int)GunplaGrade.None && value <= (int)GunplaGrade.Other;

    // La escala es opcional: null significa que el producto no la informa. Una cadena vacía o con espacios en los
    // extremos no es "no informada", es un dato mal escrito, y se rechaza igual que la etiqueta de medio de pago.
    public static bool IsValidScale(string? scale) => IsValidOptionalText(scale, MaxScaleLength);

    public static bool IsValidPartNumber(string? partNumber) => IsValidOptionalText(partNumber, MaxPartNumberLength);

    // Etiqueta que publican los facets del catálogo, para que el frontend no traduzca los enteros por su cuenta.
    public static string LabelOf(GunplaGrade grade) => grade switch
    {
        GunplaGrade.HighGrade => "High Grade (HG)",
        GunplaGrade.MasterGrade => "Master Grade (MG)",
        GunplaGrade.RealGrade => "Real Grade (RG)",
        GunplaGrade.PerfectGrade => "Perfect Grade (PG)",
        GunplaGrade.SuperDeformed => "Super Deformed (SD)",
        GunplaGrade.Other => "Otro",
        _ => "Sin grado"
    };

    private static bool IsValidOptionalText(string? value, int maxLength) =>
        value is null || (value.Length > 0 && value.Length <= maxLength && value == value.Trim());
}
