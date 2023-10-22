import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { groupDto, groupVM } from '../models/group.model';
import { createCategoryCommand } from '../models/category.model';

@Injectable({
  providedIn: 'root'
})
export class GroupsService {
  readonly groupsGetPath = environment.apiUrl + '/api/Groups';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<groupDto>>{
    return this.http.get<Array<groupDto>>(this.groupsGetPath)
  }

  GetbyId(id:string | null):Observable<groupVM>{
    return this.http.get<groupVM>(`${this.groupsGetPath}/${id}`);
  }

  Create(group:createCategoryCommand): Observable<any>{
    return this.http.post<createCategoryCommand>(this.groupsGetPath,group);
  }

  Update(id:string | null,group:groupVM):Observable<any>{
    return this.http.put(`${this.groupsGetPath}/${id}`, group);
  }

  Toggle(id:number){
    return this.http.delete(`${this.groupsGetPath}/${id}`);
  }
}
