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
    productName: string;
    storeName: string;
    discountPercentage: number;
    productViewCount: number;
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
    productReleaseDate: string;
    productViewCount: number;
    categories: string[];
    imageUrls: string[];
    storeCount: number;
    minPrice: number;
    maxPrice: number;
    offers: ProductOfferDto[];
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
