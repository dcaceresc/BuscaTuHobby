namespace Application.Maintainer.Inventories.IrionOffers;

// New branch only: absence must not silently become false or a fabricated release date.
public record NewIrionProductDetails(
    Guid NewProductId, string ProductName, Guid ManufacturerId, Guid FranchiseId, Guid? SerieId,
    bool? ProductHasBase, string ProductTargetAge, string ProductSize, string ProductDescription,
    DateOnly? ProductReleaseDate, IReadOnlyList<Guid> CategoryIds);

public record NewIrionCatalogOption(Guid Id, string Label);
public record NewIrionSerieOption(Guid Id, string Label, Guid FranchiseId);
public record NewIrionProductOptionsDto(
    IReadOnlyList<NewIrionCatalogOption> Manufacturers, IReadOnlyList<NewIrionCatalogOption> Franchises,
    IReadOnlyList<NewIrionCatalogOption> Categories, IReadOnlyList<NewIrionSerieOption> Series);
