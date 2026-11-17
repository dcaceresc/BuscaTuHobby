import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse, IngestionRunDto, IngestionStatusDto, StartIngestionRun } from '@app/core/models';

@Injectable({
  providedIn: 'root'
})
export class IngestionService {

  private readonly http = inject(HttpClient);

  public getStatus() {
    return this.http.get<ApiResponse<IngestionStatusDto[]>>('/api/maintainer/ingestion/status');
  }

  public startRun(request: StartIngestionRun) {
    return this.http.post<ApiResponse<IngestionRunDto>>('/api/maintainer/ingestion/runs', request);
  }
}
