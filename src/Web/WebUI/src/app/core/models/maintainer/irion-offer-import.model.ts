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

export interface IrionOfferConfirmationDto {
  outcome: 'Saved' | 'AlreadySaved' | 'RefreshRequired' | 'Conflict';
  inventoryId: string | null;
  freshPreview: IrionProductPreviewDto | null;
  inspection: IrionOfferInspectionDto | null;
}
