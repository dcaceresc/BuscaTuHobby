import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateRole, RoleDto, RoleVM, UpdateRole } from '../../models/security/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {

  private http = inject(HttpClient);

  public getRoles() {
    return this.http.get<ApiResponse<RoleDto[]>>('/api/security/roles');
  }

  public getRoleById(id: string | null){
    return this.http.get<ApiResponse<RoleVM>>(`/api/security/roles/${id}`);
  }

  public addRole(role: CreateRole){
    return this.http.post<ApiResponse<any>>('/api/security/roles', role);
  }

  public updateRole(id: string | null ,role: UpdateRole){
    return this.http.put<ApiResponse<any>>(`/api/security/roles/${id}`, role);
  }

  public toggleRole(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/security/roles/${id}`);
  }

}
