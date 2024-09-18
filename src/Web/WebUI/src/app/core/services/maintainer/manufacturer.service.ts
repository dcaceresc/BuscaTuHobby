import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateManufacturer, ManufacturerDto, ManufacturerVM, UpdateManufacturer } from '../../models/maintainer/manufacturer.model';

@Injectable({
  providedIn: 'root'
})
export class ManufacturerService {

  private http = inject(HttpClient);


  public getManufacturers() {
    return this.http.get<ApiResponse<ManufacturerDto[]>>('/api/manufacturers');
  }

  public getManufacturerById(id: string | null) {
    return this.http.get<ApiResponse<ManufacturerVM>>(`/api/manufacturers/${id}`);
  }

  public createManufacturer(manufacturer: CreateManufacturer) {
    return this.http.post<ApiResponse<any>>('/api/manufacturers', manufacturer);
  }

  public updateManufacturer(id : string | null, manufacturer: UpdateManufacturer){
    return this.http.put<ApiResponse<any>>(`/api/manufacturers/${id}`, manufacturer);
  }

  public toggleManufacturer(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/manufacturers/${id}`);
  }



}
