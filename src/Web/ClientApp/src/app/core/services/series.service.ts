import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { createSerieCommnad, serieDto, serieVM } from '../models/serie.model';

@Injectable({
  providedIn: 'root'
})
export class SeriesService {

  readonly SeriesGetPath = environment.apiUrl + '/api/Series';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<serieDto>>{
    return this.http.get<Array<serieDto>>(this.SeriesGetPath)
  }

  GetbyId(id:string | null):Observable<serieVM>{
    return this.http.get<serieVM>(`${this.SeriesGetPath}/${id}`);
  }

  Create(serie:createSerieCommnad): Observable<serieVM>{
    return this.http.post<serieVM>(this.SeriesGetPath,serie);
  }

  Update(id:string | null,serie:serieVM):Observable<any>{
    return this.http.put(`${this.SeriesGetPath}/${id}`, serie);
  }

  Toggle(id:number){
    return this.http.delete(`${this.SeriesGetPath}/${id}`);
  }
}
