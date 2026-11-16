/**
 * Una observación de precio agregada de un producto para un solo día.
 * `date` es una fecha de calendario ISO (`YYYY-MM-DD`).
 */
export interface PricePointDto {
  date: string;
  minPrice: number;
  maxPrice: number;
  offerCount: number;
}
