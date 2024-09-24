import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateInventory, InventoryDto, InventoryVM } from '../../models/maintainer/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {

  private http = inject(HttpClient);

  public getInventories() {
    return this.http.get<ApiResponse<InventoryDto[]>>('/api/inventories');
  }

  public getInventoryById(id: string | null) {
    return this.http.get<ApiResponse<InventoryVM>>(`/api/inventories/${id}`);
  }

  public createInventory(inventory: CreateInventory) {
    return this.http.post<ApiResponse<any>>('/api/inventories', inventory);
  }

  public updateInventory(id: string | null,inventory: InventoryVM) {
    return this.http.put<ApiResponse<any>>(`/api/inventories/${id}`, inventory);
  }

  public toggleInventory(id: string | null) {
    return this.http.delete<ApiResponse<any>>(`/api/inventories/${id}`);
  }

}
