import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreaterGroup, GroupDto, UpdateGroup } from '../../models/maintainer/group.model';

@Injectable({
  providedIn: 'root'
})
export class GroupService {

  private http = inject(HttpClient);

  public getGroups() {
    return this.http.get<ApiResponse<GroupDto[]>>('api/groups');
  }

  public getGroupById(id: string | null) {
    return this.http.get<ApiResponse<GroupDto>>(`api/groups/${id}`);
  }

  public addGroup(group: CreaterGroup) {
    return this.http.post<ApiResponse<any>>('api/groups', group);
  }

  public updateGroup(id: string | null,group: UpdateGroup) {
    return this.http.put<ApiResponse<any>>(`api/groups/${id}`, group);
  }

  public toggleGroup(id: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/groups/${id}`);
  }

}
