import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateSerie, SerieByFranchiseDto, SerieDto, SerieVM, UpdateSerie } from '../../models/maintainer/serie.model';

@Injectable({
  providedIn: 'root'
})
export class SerieService {

  private http = inject(HttpClient);


  public getSeries() {
    return this.http.get<ApiResponse<SerieDto[]>>('/api/series');
  }

  public getSerieById(id: string | null) {
    return this.http.get<ApiResponse<SerieVM>>(`/api/series/${id}`);
  }

  public getSeriesByFranchiseId(franchiseId: string | null) {
    return this.http.get<ApiResponse<SerieByFranchiseDto[]>>(`/api/series/franchise/${franchiseId}`);
  }

  public createSerie(scale: CreateSerie) {
    return this.http.post<ApiResponse<any>>('/api/series', scale);
  }

  public updateSerie(id : string | null, scale: UpdateSerie){
    return this.http.put<ApiResponse<any>>(`/api/series/${id}`, scale);
  }

  public toggleSerie(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/series/${id}`);
  }

}
