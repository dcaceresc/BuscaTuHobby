import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment.development';
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

  Create(manufacturer:manufacturerVM): Observable<any>{
    return this.http.post<createManufacturerCommand>(this.manufacturersGetPath,manufacturer);
  }

  Update(id:string | null,manufacturer:manufacturerVM):Observable<any>{
    return this.http.put(`${this.manufacturersGetPath}/${id}`, manufacturer);
  }

  Toggle(id:number){
    return this.http.delete(`${this.manufacturersGetPath}/${id}`);
  }
}
