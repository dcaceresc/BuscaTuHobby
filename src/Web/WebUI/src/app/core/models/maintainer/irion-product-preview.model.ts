export interface IrionProductPreviewDto {
  sourceUrl: string;
  title: string;
  vendor: string;
  currency: string;
  descriptionHtml: string;
  tags: string[];
  imageUrls: string[];
  available: boolean;
  variants: IrionVariantPreviewDto[];
}

export interface IrionVariantPreviewDto {
  id: string;
  sku: string | null;
  barcode: string | null;
  available: boolean;
  price: number;
  originalPrice: number | null;
  discountPercentage: number | null;
}
