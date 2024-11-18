import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse, DashboardMenuDto } from '@app/core/models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private http = inject(HttpClient);


  public getMenu() {
    return this.http.get<ApiResponse<DashboardMenuDto[]>>('api/dashboard/getMenu');
  }

}
