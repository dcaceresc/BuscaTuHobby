import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '@app/core/models';
import { MenuCategoryDto, MenuStoreDto, MostSearchedProductDto, RecentPostDto, BestDealDto, PopularCategoryDto, FeaturedStoreDto, RecentActivityDto, SearchProductDto } from '@app/core/models/dashboard/dashboard.model';

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
}
