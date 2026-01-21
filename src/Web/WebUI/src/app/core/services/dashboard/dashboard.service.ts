import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '@app/core/models';
import { MenuCategoryDto, MenuStoreDto } from '@app/core/models/dashboard/dashboard.model';

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
}
