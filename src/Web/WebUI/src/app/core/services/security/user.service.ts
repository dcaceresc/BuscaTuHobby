import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { UserDto } from '../../models/security/user.model';
import { ApiResponse } from '../../models/apiResponse.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private http = inject(HttpClient);

  public getUsers() {
    return this.http.get<ApiResponse<UserDto[]>>('/api/security/users');
  }

}
