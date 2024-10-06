import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { ConfigurationDto, ConfigurationVM, CreateConfiguration, UpdateConfiguration } from '../../models/maintainer/configuration.model';

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {

  private http = inject(HttpClient);

  public getConfigurations() {
    return this.http.get<ApiResponse<ConfigurationDto[]>>('api/maintainer/configurations');
  }

  public getConfigurationById(id: string | null) {
    return this.http.get<ApiResponse<ConfigurationVM>>(`api/maintainer/configurations/${id}`);
  }

  public addConfiguration(configuration: CreateConfiguration) {
    return this.http.post<ApiResponse<any>>('api/maintainer/configurations', configuration);
  }

  public updateConfiguration(id: string | null, configuration: UpdateConfiguration) {
    return this.http.put<ApiResponse<any>>(`api/maintainer/configurations/${id}`, configuration);
  }

  public toggleConfiguration(id: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/maintainer/configurations/${id}`);
  }

}
