import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { createScaleCommnad, scaleDto, scaleVM } from '../models/scale.model';

@Injectable({
  providedIn: 'root'
})
export class ScalesService {
  
  readonly ScalesGetPath = environment.apiUrl + '/api/Scales';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<scaleDto>>{
    return this.http.get<Array<scaleDto>>(this.ScalesGetPath)
  }

  GetbyId(id:string | null):Observable<scaleVM>{
    return this.http.get<scaleVM>(`${this.ScalesGetPath}/${id}`);
  }

  Create(scale : createScaleCommnad): Observable<any>{
    return this.http.post<createScaleCommnad>(this.ScalesGetPath,scale);
  }

  Update(id:string | null,scale:scaleVM):Observable<any>{
    return this.http.put(`${this.ScalesGetPath}/${id}`, scale);
  }

  Toggle(id:number){
    return this.http.delete(`${this.ScalesGetPath}/${id}`);
  }
}