import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { GroupDto } from '../../models/maintainer/group.model';

@Injectable({
  providedIn: 'root'
})
export class GroupService {

  private http = inject(HttpClient);

  public getGroups() {
    return this.http.get<ApiResponse<GroupDto[]>>('api/groups');
  }

}
