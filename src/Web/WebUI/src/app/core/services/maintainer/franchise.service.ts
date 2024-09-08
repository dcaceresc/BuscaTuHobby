import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateFranchise, FranchiseDto, FranchiseVM, UpdateFranchise } from '../../models/maintainer/franchise.model';

@Injectable({
  providedIn: 'root'
})
export class FranchiseService {

  private http = inject(HttpClient);

  public getFranchises() {
    return this.http.get<ApiResponse<FranchiseDto[]>>('api/franchises');
  }

  public getFranchise(id: string | null) {
    return this.http.get<ApiResponse<FranchiseVM>>(`api/franchises/${id}`);
  }

  public addFranchise(franchise: CreateFranchise) {
    return this.http.post<ApiResponse<any>>('api/franchises', franchise);
  }

  public updateFranchise(id: string | null, franchise: UpdateFranchise) {
    return this.http.put<ApiResponse<any>>(`api/franchises/${id}`, franchise);
  }

  public toggleFranchise(id: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/franchises/${id}`);
  }
}
