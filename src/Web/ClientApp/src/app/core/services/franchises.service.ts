import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment.development';
import { createFranchiseCommand, franchiseDto, franchiseVM } from '../models/franchise.model';

@Injectable({
  providedIn: 'root'
})
export class FranchisesService {
  readonly franchisesGetPath = environment.apiUrl + '/api/Franchises';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<franchiseDto>>{
    return this.http.get<Array<franchiseDto>>(this.franchisesGetPath)
  }

  GetbyId(id:string | null):Observable<franchiseVM>{
    return this.http.get<franchiseVM>(`${this.franchisesGetPath}/${id}`);
  }

  Create(franchise:createFranchiseCommand): Observable<any>{
    return this.http.post<createFranchiseCommand>(this.franchisesGetPath,franchise);
  }

  Update(id:string | null,franchise:franchiseVM):Observable<any>{
    return this.http.put(`${this.franchisesGetPath}/${id}`, franchise);
  }

  Toggle(id:number){
    return this.http.delete(`${this.franchisesGetPath}/${id}`);
  }
}
