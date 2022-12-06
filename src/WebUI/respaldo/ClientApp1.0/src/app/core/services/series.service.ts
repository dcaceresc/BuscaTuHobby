import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SerieVm } from 'src/app/shared/models/serie-vm';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SeriesService {

  readonly SeriesGetPath = environment.apiUrl + '/api/Series';

  constructor(
    private http:HttpClient
  ) { }

  GetAll():Observable<Array<SerieVm>>{
    return this.http.get<Array<SerieVm>>(this.SeriesGetPath)
  }
}
