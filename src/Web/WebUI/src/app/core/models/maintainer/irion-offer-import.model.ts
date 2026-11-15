import { IrionProductPreviewDto } from './irion-product-preview.model';

export interface IrionOfferSelection {
  productId: string;
  storeId: string;
  sourceUrl: string;
  externalVariantId: string;
}

export interface IrionOfferInspectionDto extends IrionOfferSelection {
  outcome: 'Create' | 'Update' | 'Conflict';
  inventoryId: string | null;
  rowVersion: string | null;
  isActive: boolean | null;
  message: string | null;
}

export interface ConfirmIrionOffer extends IrionOfferSelection {
  expectedInventoryId: string | null;
  expectedRowVersion: string | null;
  expectedOffer: {
    currency: string;
    price: number;
    originalPrice: number | null;
    available: boolean;
  };
}

export interface NewIrionProductDetails {
  newProductId: string;
  productName: string;
  manufacturerId: string;
  franchiseId: string;
  serieId: string | null;
  productHasBase: boolean;
  productTargetAge: string;
  productSize: string;
  productDescription: string;
  productReleaseDate: string;
  categoryIds: string[];
}

export interface NewIrionProductOptionsDto {
  manufacturers: { id: string; label: string }[];
  franchises: { id: string; label: string }[];
  categories: { id: string; label: string }[];
  series: { id: string; label: string; franchiseId: string }[];
}

export interface InspectNewIrionProductOffer {
  product: NewIrionProductDetails;
  storeId: string;
  sourceUrl: string;
  externalVariantId: string;
}

export interface ConfirmNewIrionProductOffer extends InspectNewIrionProductOffer {
  expectedInventoryId: string | null;
  expectedRowVersion: string | null;
  expectedOffer: ConfirmIrionOffer['expectedOffer'];
}

export interface IrionOfferConfirmationDto {
  outcome: 'Saved' | 'AlreadySaved' | 'RefreshRequired' | 'Conflict';
  inventoryId: string | null;
  freshPreview: IrionProductPreviewDto | null;
  inspection: IrionOfferInspectionDto | null;
}
