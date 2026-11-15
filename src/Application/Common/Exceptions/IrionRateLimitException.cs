namespace Application.Common.Exceptions;

public sealed class IrionRateLimitException(string? retryGuidance = null) : Exception(
    "Irion está limitando las consultas (HTTP 429). " + (retryGuidance ?? "Inténtalo más tarde."));

public sealed class IrionProductUrlException() : Exception(
    "Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto");
