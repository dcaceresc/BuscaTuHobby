import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CategoryDto, CategoryVM, CreateCategory, UpdateCategory } from '../../models/maintainer/category.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private http = inject(HttpClient);


  public getCategories() {
    return this.http.get<ApiResponse<CategoryDto[]>>('api/maintainer/categories');
  }

  public getCategoryById(categoryId: string | null) {
    return this.http.get<ApiResponse<CategoryVM>>(`api/maintainer/categories/${categoryId}`);
  }

  public createCategory(category: CreateCategory) {
    return this.http.post<ApiResponse<any>>('api/maintainer/categories', category);
  }

  public updateCategory(categoryId: string | null, category: UpdateCategory) {
    return this.http.put<ApiResponse<any>>(`api/maintainer/categories/${categoryId}`, category);
  }

  public toggleCategory(categoryId: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/maintainer/categories/${categoryId}`);
  }

}
