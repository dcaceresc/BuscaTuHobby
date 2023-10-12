import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { createStoreCommand, storeDto } from '../models/store.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StoresService {
  
  readonly StoresGetPath = environment.apiUrl + '/api/Stores';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<storeDto>>{
    return this.http.get<Array<storeDto>>(this.StoresGetPath)
  }

  GetbyId(id:string | null):Observable<storeDto>{
    return this.http.get<storeDto>(`${this.StoresGetPath}/${id}`);
  }

  Create(universe:createStoreCommand): Observable<any>{
    return this.http.post<createStoreCommand>(this.StoresGetPath,universe);
  }

  Update(id:string | null,universe:storeDto):Observable<any>{
    return this.http.put(`${this.StoresGetPath}/${id}`, universe);
  }

  Toggle(id:number){
    return this.http.delete(`${this.StoresGetPath}/${id}`);
  }
}