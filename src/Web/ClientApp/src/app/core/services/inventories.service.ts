import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { createInventoryCommand, inventoryDto, inventoryVM } from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoriesService {
  readonly inventoriesGetPath = environment.apiUrl + '/api/Inventories';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<inventoryDto>>{
    return this.http.get<Array<inventoryDto>>(this.inventoriesGetPath)
  }

  GetbyId(id:string | null):Observable<inventoryVM>{
    return this.http.get<inventoryVM>(`${this.inventoriesGetPath}/${id}`);
  }

  Create(inventory:createInventoryCommand): Observable<any>{
    return this.http.post<createInventoryCommand>(this.inventoriesGetPath,inventory);
  }

  Update(id:string | null,inventory:inventoryVM):Observable<any>{
    return this.http.put(`${this.inventoriesGetPath}/${id}`, inventory);
  }

  Toggle(id:number){
    return this.http.delete(`${this.inventoriesGetPath}/${id}`);
  }
}