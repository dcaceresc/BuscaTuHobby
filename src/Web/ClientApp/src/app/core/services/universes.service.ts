import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { universeVM } from '../models/universe.model';

@Injectable({
  providedIn: 'root'
})
export class UniversesService {

  readonly UniversesGetPath = environment.apiUrl + '/api/Universes';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<universeVM>>{
    return this.http.get<Array<universeVM>>(this.UniversesGetPath)
  }

  GetbyId(id:string | null):Observable<universeVM>{
    return this.http.get<universeVM>(`${this.UniversesGetPath}/${id}`);
  }

  Create(universe:universeVM): Observable<universeVM>{
    return this.http.post<universeVM>(this.UniversesGetPath,universe);
  }

  Update(id:string | null,universe:universeVM):Observable<any>{
    return this.http.put(`${this.UniversesGetPath}/${id}`, universe);
  }

  Toggle(id:number){
    return this.http.delete(`${this.UniversesGetPath}/${id}`);
  }


}
