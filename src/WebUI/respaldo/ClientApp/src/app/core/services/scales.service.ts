import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ScaleVm } from 'src/app/shared/models/scale-vm';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ScalesService {

  readonly ScalesGetPath = environment.apiUrl + '/api/Scales';

  constructor(
    private http:HttpClient
  ) { }

  Get():Observable<Array<ScaleVm>>{
    return this.http.get<Array<ScaleVm>>(this.ScalesGetPath)
  }
}
