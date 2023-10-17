import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { createManufacturerCommand, manufacturerDto, manufacturerVM } from '../models/manufacturer.model';

@Injectable({
  providedIn: 'root'
})
export class ManufacturersService {
  readonly manufacturersGetPath = environment.apiUrl + '/api/Manufacturers';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<manufacturerDto>>{
    return this.http.get<Array<manufacturerDto>>(this.manufacturersGetPath)
  }

  GetbyId(id:string | null):Observable<manufacturerVM>{
    return this.http.get<manufacturerVM>(`${this.manufacturersGetPath}/${id}`);
  }

  Create(universe:manufacturerVM): Observable<any>{
    return this.http.post<createManufacturerCommand>(this.manufacturersGetPath,universe);
  }

  Update(id:string | null,universe:manufacturerVM):Observable<any>{
    return this.http.put(`${this.manufacturersGetPath}/${id}`, universe);
  }

  Toggle(id:number){
    return this.http.delete(`${this.manufacturersGetPath}/${id}`);
  }
}
