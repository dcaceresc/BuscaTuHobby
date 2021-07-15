import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UniverseVm } from 'src/app/shared/models/universe-vm';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UniversesService {

  readonly UniversesGetPath = environment.apiUrl + '/api/Universes';

  constructor(
    private http:HttpClient
  ) { }


  Get():Observable<Array<UniverseVm>>{
    return this.http.get<Array<UniverseVm>>(this.UniversesGetPath)
  }
}
