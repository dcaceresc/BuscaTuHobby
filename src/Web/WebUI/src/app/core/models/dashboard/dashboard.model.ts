import { formatDate } from '@angular/common';

export interface MenuCategoryDto{
    categoryName: string;
    categoryIcon: string;
    categorySlug: string;
}

export interface MenuStoreDto{
    storeName:string;
    storeIcon:string;
    storeSlug:string;
}

export interface RecentPostDto{
    postId: string;
    postTitle: string;
    postContent: string;
    postTypeName: string;
    postViewCount: number;
}

export interface MostSearchedProductDto{
    productId: string;
    productName: string;
    productDescription: string;
    productViewCount: number;
    storeCount: number;
    storeNames: string[];
}

export interface BestDealDto{
    inventoryId: string;
    productId: string;
    productName: string;
    storeName: string;
    discountPercentage: number;
    productViewCount: number;
}

// Una bajada de precio compara los dos últimos puntos de historial de una oferta: no hay punto anterior, no hay bajada.
export interface PriceDropDto{
    inventoryId: string;
    productId: string;
    productName: string;
    storeName: string;
    storeSlug: string;
    sourceUrl: string | null;
    currentPrice: number;
    previousPrice: number;
    dropPercentage: number;
    observedAtUtc: string;
    previousObservedAtUtc: string;
}

export interface PopularCategoryDto{
    categoryName: string;
    productCount: number;
    percentage: number;
}

export interface FeaturedStoreDto{
    storeId: string;
    storeName: string;
    storeIcon: string;
    storeWebSite: string;
    productCount: number;
    offerCount: number;
}

export interface RecentActivityDto{
    activityType: string;
    title: string;
    description: string;
    createdAt: string;
}

export interface SearchProductDto{
    productId: string;
    productName: string;
    productDescription: string;
    manufacturerName: string;
    franchiseName: string;
    productViewCount: number;
    storeCount: number;
    minPrice: number;
    maxPrice: number;
    bestDiscount: number;
    categories: string[];
}

export interface ProductOfferDto{
    inventoryId: string;
    storeId: string;
    storeName: string;
    storeSlug: string;
    storeIcon: string;
    storeWebSite: string;
    price: number;
    originalPrice: number;
    discountPercentage: number;
    availability: boolean | null;
    sourceUrl: string | null;
    observedAtUtc: string | null;
    // La condición viaja como entero desde la API: 0 nueva, 1 reutilizada, 2 usada.
    condition: number;
    paymentMethodLabel: string | null;
}

export interface ProductDetailDto{
    productId: string;
    productName: string;
    productDescription: string;
    manufacturerName: string;
    franchiseName: string;
    serieName: string | null;
    productHasBase: boolean;
    productTargetAge: string;
    productSize: string;
    // Campos estructurados del catálogo Gunpla; el grado viaja como entero (ver GUNPLA_GRADE_OPTIONS).
    grade: number;
    scale: string | null;
    partNumber: string | null;
    productReleaseDate: string;
    productViewCount: number;
    categories: string[];
    imageUrls: string[];
    storeCount: number;
    minPrice: number;
    maxPrice: number;
    offers: ProductOfferDto[];
}

export interface FilterOptionDto{
    value: string;
    label: string;
    count: number;
}

export interface CatalogProductDto{
    productId: string;
    productName: string;
    manufacturerName: string;
    franchiseName: string;
    grade: number;
    scale: string | null;
    partNumber: string | null;
    // Los precios describen solo las ofertas elegibles del producto: null significa que hoy no tiene ninguna, y es
    // distinto de un precio cero.
    minPrice: number | null;
    maxPrice: number | null;
    storeCount: number;
    imageUrls: string[];
}

/**
 * Estado completo que el catálogo envía a la API. Los filtros viajan también en la URL para que un enlace
 * o una recarga reproduzcan la misma vista.
 */
export interface CategoryProductsQuery{
    page: number;
    pageSize: number;
    sort: string;
    grade?: string;
    manufacturerId?: string;
    franchiseId?: string;
    minPrice?: string;
    maxPrice?: string;
}

export interface CategoryProductsDto{
    categoryName: string;
    categorySlug: string;
    total: number;
    page: number;
    pageSize: number;
    items: CatalogProductDto[];
    manufacturers: FilterOptionDto[];
    franchises: FilterOptionDto[];
    grades: FilterOptionDto[];
}

export interface StoreOfferDto{
    inventoryId: string;
    productId: string;
    productName: string;
    price: number;
    originalPrice: number;
    discountPercentage: number;
    // La condición viaja como entero desde la API: 0 nueva, 1 reutilizada, 2 usada.
    condition: number;
    paymentMethodLabel: string | null;
    observedAtUtc: string | null;
}

export interface StoreDetailDto{
    storeId: string;
    storeName: string;
    storeSlug: string;
    storeIcon: string;
    storeWebSite: string;
    productCount: number;
    offerCount: number;
    offers: StoreOfferDto[];
}

// La condición de la oferta llega como entero desde la API: 0 nueva, 1 reutilizada, 2 usada.
const OFFER_CONDITION_LABELS: Record<number, string> = { 0: 'Nueva', 1: 'Reutilizada', 2: 'Usada' };

/** Traduce la condición numérica de una oferta a su etiqueta en español. */
export function offerConditionLabel(condition: number): string {
    return OFFER_CONDITION_LABELS[condition] ?? 'Sin dato';
}

/**
 * La API envía la observación sin zona horaria; como el campo se llama `observedAtUtc` se interpreta como UTC
 * para que la fecha mostrada no se desplace con la zona del navegador.
 */
export function formatObservedAt(value: string): string {
    const iso = /(Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`;
    return formatDate(iso, 'd MMM y, HH:mm', 'es-CL', 'UTC');
}

export interface ProductPricePointDto{
    date: string;
    minPrice: number;
    maxPrice: number;
    offerCount: number;
}

export interface ProductPriceHistoryDto{
    productId: string;
    productName: string;
    hasHistory: boolean;
    points: ProductPricePointDto[];
}
