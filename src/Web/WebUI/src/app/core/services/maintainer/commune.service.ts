import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CommuneByRegion, CommuneDto, CommuneVM, CreateCommune, UpdateCommune } from '../../models/maintainer/commune.model';

@Injectable({
  providedIn: 'root'
})
export class CommuneService {

  private http = inject(HttpClient);

  public getCommunes() {
    return this.http.get<ApiResponse<CommuneDto[]>>('/api/communes');
  }

  public getCommuneById(id: string | null) {
    return this.http.get<ApiResponse<CommuneVM>>(`/api/communes/${id}`);
  }

  public getCommunesByRegionId(regionId: string | null) {
    return this.http.get<ApiResponse<CommuneByRegion[]>>(`/api/communes/region/${regionId}`);
  }

  public createCommune(region: CreateCommune) {
    return this.http.post<ApiResponse<any>>('/api/communes', region);
  }

  public updateCommune(id : string | null, region: UpdateCommune){
    return this.http.put<ApiResponse<any>>(`/api/communes/${id}`, region);
  }

  public toggleCommune(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/communes/${id}`);
  }

}
