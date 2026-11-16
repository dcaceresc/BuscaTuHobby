/**
 * One aggregated price observation of a product for a single day.
 * `date` is an ISO calendar date (`YYYY-MM-DD`).
 */
export interface PricePointDto {
  date: string;
  minPrice: number;
  maxPrice: number;
  offerCount: number;
}
