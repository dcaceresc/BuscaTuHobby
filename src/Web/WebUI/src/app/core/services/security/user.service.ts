import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { CreateUser, UserDto, UserVM } from '../../models/security/user.model';
import { ApiResponse } from '../../models/apiResponse.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private http = inject(HttpClient);

  public getUsers() {
    return this.http.get<ApiResponse<UserDto[]>>('/api/security/users');
  }

  public getUserById(userId: string | null) {
    return this.http.get<ApiResponse<UserVM>>(`/api/security/users/${userId}`);
  }

  public createUser(user: CreateUser) {
    return this.http.post<ApiResponse<any>>('/api/security/users', user);
  }

  public updateUser(user: UserVM) {
    return this.http.put<ApiResponse<any>>(`/api/security/users/${user.userId}`, user);
  }

  public toggleUser(userId: string) {
    return this.http.delete<ApiResponse<any>>(`/api/security/users/${userId}`);
  }

}
