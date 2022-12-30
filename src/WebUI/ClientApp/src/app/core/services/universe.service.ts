import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/app/shared/models/environment';
import { UniverseVm } from 'src/app/shared/models/universe-vm';

@Injectable({
  providedIn: 'root'
})
export class UniverseService {

  readonly UniversesGetPath = environment.apiUrl + '/api/Universes';

  constructor(
    private http:HttpClient
  ) { }


  GetAll():Observable<Array<UniverseVm>>{
    return this.http.get<Array<UniverseVm>>(this.UniversesGetPath)
  }
}
