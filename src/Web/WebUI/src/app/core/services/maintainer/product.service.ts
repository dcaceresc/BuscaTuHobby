import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateProduct, ProductDto, ProductVM } from '../../models/maintainer/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private http = inject(HttpClient);


  public getProducts() {
    return this.http.get<ApiResponse<ProductDto[]>>('/api/maintainer/products');
  }

  public getProductById(id: string | null) {
    return this.http.get<ApiResponse<ProductVM>>(`/api/maintainer/products/${id}`);
  }

  public createProduct(product: CreateProduct) {

    return this.http.post<ApiResponse<string>>('/api/maintainer/products', product);
  }

  public createProductImages(id: string | null, images: FormData) {
    return this.http.post<ApiResponse<any>>(`/api/maintainer/products/${id}/images`, images);
  }

  public updateProduct(id : string | null,product: ProductDto) {
    return this.http.put<ApiResponse<any>>(`/api/maintainer/products/${id}`, product);
  }

  public toggleProduct(id: string | null) {
    return this.http.delete<ApiResponse<any>>(`/api/maintainer/products/${id}`);
  }

}
