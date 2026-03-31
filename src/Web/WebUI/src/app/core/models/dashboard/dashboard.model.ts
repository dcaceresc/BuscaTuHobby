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