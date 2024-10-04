import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateSubMenu, SubMenuDto, SubMenuVM, UpdateSubMenu } from '../../models/maintainer/submenu.model';

@Injectable({
  providedIn: 'root'
})
export class SubMenuService {

  private http = inject(HttpClient);


  public getSubMenus() {
    return this.http.get<ApiResponse<SubMenuDto[]>>('/api/submenus');
  }

  public getSubMenuById(id: string | null) {
    return this.http.get<ApiResponse<SubMenuVM>>(`/api/submenus/${id}`);
  }

  public createSubMenu(scale: CreateSubMenu) {
    return this.http.post<ApiResponse<any>>('/api/submenus', scale);
  }

  public updateSubMenu(id : string | null, scale: UpdateSubMenu){
    return this.http.put<ApiResponse<any>>(`/api/submenus/${id}`, scale);
  }

  public toggleSubMenu(id: string | null){
    return this.http.delete<ApiResponse<any>>(`/api/submenus/${id}`);
  }

}
