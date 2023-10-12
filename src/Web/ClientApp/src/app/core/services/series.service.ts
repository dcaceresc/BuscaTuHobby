import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { serieVM } from '../models/serie.model';

@Injectable({
  providedIn: 'root'
})
export class SeriesService {

  readonly SeriesGetPath = environment.apiUrl + '/api/Series';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<serieVM>>{
    return this.http.get<Array<serieVM>>(this.SeriesGetPath)
  }

  GetbyId(id:string | null):Observable<serieVM>{
    return this.http.get<serieVM>(`${this.SeriesGetPath}/${id}`);
  }

  Create(universe:serieVM): Observable<serieVM>{
    return this.http.post<serieVM>(this.SeriesGetPath,universe);
  }

  Update(id:string | null,universe:serieVM):Observable<any>{
    return this.http.put(`${this.SeriesGetPath}/${id}`, universe);
  }

  Toggle(id:number){
    return this.http.delete(`${this.SeriesGetPath}/${id}`);
  }
}
