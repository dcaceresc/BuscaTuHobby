import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse } from '../../models/apiResponse.model';
import { CreateMenu, MenuDto, MenuVM, UpdateMenu } from '../../models/maintainer/menu.model';

@Injectable({
  providedIn: 'root'
})
export class MenuService {

  private http = inject(HttpClient);

  public getMenus() {
    return this.http.get<ApiResponse<MenuDto[]>>('api/maintainer/menus');
  }

  public getMenuById(id: string | null) {
    return this.http.get<ApiResponse<MenuVM>>(`api/maintainer/menus/${id}`);
  }

  public createMenu(group: CreateMenu) {
    return this.http.post<ApiResponse<any>>('api/maintainer/menus', group);
  }

  public updateMenu(id: string | null,group: UpdateMenu) {
    return this.http.put<ApiResponse<any>>(`api/maintainer/menus/${id}`, group);
  }

  public toggleMenu(id: string | null) {
    return this.http.delete<ApiResponse<any>>(`api/maintainer/menus/${id}`);
  }

}
