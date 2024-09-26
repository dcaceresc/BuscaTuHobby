import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateRegion, RegionDto, RegionVM, UpdateRegion } from '../../models/maintainer/region.model';

@Injectable({
  providedIn: 'root'
})
export class RegionService {

  private http = inject(HttpClient);

  public getRegions() {
    return this.http.get<ApiResponse<RegionDto[]>>('/api/regions');
  }

  public getRegionById(id: string | null) {
    return this.http.get<ApiResponse<RegionVM>>(`/api/regions/${id}`);
  }

  public createRegion(region: CreateRegion) {
    return this.http.post<ApiResponse<any>>('/api/regions', region);
  }

  public updateRegion(id : string | null, region: UpdateRegion){
    return this.http.put<ApiResponse<any>>(`/api/regions/${id}`, region);
  }

  public toggleRegion(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/regions/${id}`);
  }

}
