import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ManufacturerVm } from 'src/app/shared/models/manufacturer-vm';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ManufacturersService {

  readonly ManufacturersGetPath = environment.apiUrl + '/api/Manufacturers';

  constructor(
    private http:HttpClient
  ) { }

  Get():Observable<Array<ManufacturerVm>>{
    return this.http.get<Array<ManufacturerVm>>(this.ManufacturersGetPath)
  }
}
