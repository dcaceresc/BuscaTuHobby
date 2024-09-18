import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateScale, ScaleDto, ScaleVM, UpdateScale } from '../../models/maintainer/scale.model';

@Injectable({
  providedIn: 'root'
})
export class ScaleService {

  private http = inject(HttpClient);


  public getScales() {
    return this.http.get<ApiResponse<ScaleDto[]>>('/api/scales');
  }

  public getScaleById(id: string | null) {
    return this.http.get<ApiResponse<ScaleVM>>(`/api/scales/${id}`);
  }

  public createScale(scale: CreateScale) {
    return this.http.post<ApiResponse<any>>('/api/scales', scale);
  }

  public updateScale(id : string | null, scale: UpdateScale){
    return this.http.put<ApiResponse<any>>(`/api/scales/${id}`, scale);
  }

  public toggleScale(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/scales/${id}`);
  }

}
