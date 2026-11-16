import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '@app/core/models';
import { MenuCategoryDto, MenuStoreDto, MostSearchedProductDto, RecentPostDto, BestDealDto, PopularCategoryDto, FeaturedStoreDto, RecentActivityDto, SearchProductDto, ProductDetailDto, ProductPriceHistoryDto, CategoryProductsDto, CategoryProductsQuery, StoreDetailDto } from '@app/core/models/dashboard/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private http = inject(HttpClient);

  public getMenuCategories() {
    return this.http.get<ApiResponse<MenuCategoryDto[]>>('api/dashboard/menu-categories');
  }

  public getMenuStores(){
    return this.http.get<ApiResponse<MenuStoreDto[]>>('api/dashboard/menu-stores');
  }

  public getRecentPosts() {
    return this.http.get<ApiResponse<RecentPostDto[]>>('api/dashboard/recent-posts');
  }

  public incrementPostViewCount(postId: string) {
    return this.http.put<ApiResponse<void>>(`api/dashboard/posts/${postId}/view`, {});
  }

  public getMostSearchedProducts() {
    return this.http.get<ApiResponse<MostSearchedProductDto[]>>('api/dashboard/most-searched-products');
  }

  public incrementProductViewCount(productId: string) {
    return this.http.put<ApiResponse<void>>(`api/dashboard/products/${productId}/view`, {});
  }

  public getBestDeals() {
    return this.http.get<ApiResponse<BestDealDto[]>>('api/dashboard/best-deals');
  }

  public getPopularCategories() {
    return this.http.get<ApiResponse<PopularCategoryDto[]>>('api/dashboard/popular-categories');
  }

  public getFeaturedStores() {
    return this.http.get<ApiResponse<FeaturedStoreDto[]>>('api/dashboard/featured-stores');
  }

  public getRecentActivity() {
    return this.http.get<ApiResponse<RecentActivityDto[]>>('api/dashboard/recent-activity');
  }

  public searchProducts(term: string) {
    return this.http.get<ApiResponse<SearchProductDto[]>>(`api/dashboard/search-products?term=${encodeURIComponent(term)}`);
  }

  public getProductDetail(productId: string) {
    return this.http.get<ApiResponse<ProductDetailDto>>(`api/dashboard/products/${encodeURIComponent(productId)}`);
  }

  public getProductPriceHistory(productId: string, days = 0) {
    return this.http.get<ApiResponse<ProductPriceHistoryDto>>(
      `api/dashboard/products/${encodeURIComponent(productId)}/price-history?days=${days}`
    );
  }

  /**
   * Catálogo público de una categoría. Los filtros viajan como parámetros de consulta para que la API responda
   * exactamente la vista que la URL describe.
   */
  public getCategoryProducts(slug: string, query: CategoryProductsQuery) {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize)
      .set('sort', query.sort);

    if (query.grade) {
      params = params.set('grade', query.grade);
    }
    if (query.manufacturerId) {
      params = params.set('manufacturerId', query.manufacturerId);
    }
    if (query.franchiseId) {
      params = params.set('franchiseId', query.franchiseId);
    }
    if (query.minPrice) {
      params = params.set('minPrice', query.minPrice);
    }
    if (query.maxPrice) {
      params = params.set('maxPrice', query.maxPrice);
    }

    return this.http.get<ApiResponse<CategoryProductsDto>>(
      `api/dashboard/categories/${encodeURIComponent(slug)}/products`,
      { params }
    );
  }

  public getStoreDetail(slug: string) {
    return this.http.get<ApiResponse<StoreDetailDto>>(`api/dashboard/stores/${encodeURIComponent(slug)}`);
  }
}
