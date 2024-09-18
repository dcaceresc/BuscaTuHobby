import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateStore, StoreDto, StoreVM, UpdateStore } from '../../models/maintainer/store.model';

@Injectable({
  providedIn: 'root'
})
export class StoreService {

  private http = inject(HttpClient);


  public getStores() {
    return this.http.get<ApiResponse<StoreDto[]>>('/api/stores');
  }

  public getStoreById(id: string | null) {
    return this.http.get<ApiResponse<StoreVM>>(`/api/stores/${id}`);
  }

  public createStore(store: CreateStore) {
    return this.http.post<ApiResponse<any>>('/api/stores', store);
  }

  public updateStore(id : string | null, store: UpdateStore){
    return this.http.put<ApiResponse<any>>(`/api/stores/${id}`, store);
  }

  public toggleStore(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/stores/${id}`);
  }

}
