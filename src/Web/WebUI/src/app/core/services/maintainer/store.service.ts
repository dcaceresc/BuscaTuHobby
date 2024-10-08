import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateStore, CreateStoreAddress, StoreDto, StoreVM, UpdateStore, UpdateStoreAddress } from '../../models/maintainer/store.model';

@Injectable({
  providedIn: 'root'
})
export class StoreService {

  private http = inject(HttpClient);


  public getStores() {
    return this.http.get<ApiResponse<StoreDto[]>>('/api/maintainer/stores');
  }

  public getStoreById(id: string | null) {
    return this.http.get<ApiResponse<StoreVM>>(`/api/maintainer/stores/${id}`);
  }

  public createStore(store: CreateStore) {
    return this.http.post<ApiResponse<string>>('/api/maintainer/stores', store);
  }

  public createStoreAddress(storeId:string | null, address: CreateStoreAddress){
    return this.http.post<ApiResponse<any>>(`/api/maintainer/stores/${storeId}/address`, address);
  }

  public updateStore(id : string | null, store: UpdateStore){
    return this.http.put<ApiResponse<any>>(`/api/maintainer/stores/${id}`, store);
  }

  public updateStoreAddress(storeId:string | null , addressId: string, address: UpdateStoreAddress){
    return this.http.put<ApiResponse<any>>(`/api/maintainer/stores/${storeId}/address/${addressId}`, address);
  }

  public toggleStore(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/maintainer/stores/${id}`);
  }

  public deleteStoreAddress(storeId: string | null, addressId: string){
    return this.http.delete<ApiResponse<any>>(`/api/maintainer/stores/${storeId}/address/${addressId}`);
  }

}
