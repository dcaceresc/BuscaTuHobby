import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, map, Observable, of } from 'rxjs';
import { ApiResponse } from '../../models/apiResponse.model';
import { AdminLoginRequestCommand, ConfirmEmail, UserLogin, UserRegister, UserTokenResponse } from '../../models/security/account.model';

@Injectable({
  providedIn: 'root'
})
export class AuthorizeService {

  private router = inject(Router);
  private http = inject(HttpClient);

  private userSubject: BehaviorSubject<UserTokenResponse | null>;
  public user: Observable<UserTokenResponse | null>;


  constructor() {
    this.userSubject = new BehaviorSubject(JSON.parse(sessionStorage.getItem('user')!));
    this.user = this.userSubject.asObservable();
  }

  public get userValue(): UserTokenResponse | null {
    return this.userSubject.value;
  }

  public set userValue(user: UserTokenResponse | null) {
    sessionStorage.setItem('user', JSON.stringify(user));
    this.userSubject.next(user);
  }

  public isAuthenticated(): Observable<boolean> {
    return this.user.pipe(
      map(user => user !== null)
    );
  }


  public login(user : UserLogin) {
    return this.http.post<ApiResponse<UserTokenResponse>>('/api/security/account/userLogin',user )
        .pipe(map(response => {
            if (response.success) {
              this.userValue = response.data;
            }
            return response;
        }));
  }

  public adminLogin(admin : AdminLoginRequestCommand) {
    return this.http.post<ApiResponse<UserTokenResponse>>('/api/Security/Account/AdminLogin',admin)
        .pipe(map(response => {
          if (response.success) {
            this.userValue = response.data;
          }
          return response;
        }));
  }

  public register(user : UserRegister) {
    return this.http.post<ApiResponse<any>>('/api/security/account/UserRegister',user);
  }

  public confirmEmail(user: ConfirmEmail) {
    return this.http.post<ApiResponse<any>>('/api/security/account/ConfirmEmail', user);
  }

  public logout(): void{
    sessionStorage.removeItem('user');
    this.userSubject.next(null);
    this.router.navigate(['']);
  }

  public refreshToken() : Observable<ApiResponse<UserTokenResponse>> {
    const user = sessionStorage.getItem('user');

    if (user) {
      const refreshToken = JSON.parse(user).refreshToken;
      return this.http.post<ApiResponse<UserTokenResponse>>('/api/Security/Account/RefreshToken', { refreshToken })
        .pipe(map((response) => {

          if (response.success) {
            sessionStorage.setItem('user', JSON.stringify(response.data));
            this.userSubject.next(response.data);

          }

          return response;
        }),
        catchError(() => of({
          success: false,
          message: 'Error al refrescar el token',
        } as ApiResponse<UserTokenResponse>))
      );
    }

    return of({
      success: false,
      message: 'No se encontró el usuario en sessionStorage',
    } as ApiResponse<UserTokenResponse>);
  }

  public getToken() : string | null {
    const user = sessionStorage.getItem('user');
    return user ? JSON.parse(user).accessToken : null;
  }

  private getDecodedTokenClaim(claim: string): string[] {
    const token = this.getToken();

    if (token) {
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      return decodedToken[claim] || [];
    }

    return [];
  }

  public getRoles(): string[] {
    return this.getDecodedTokenClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
  }

  public isAdmin(): boolean {
    return this.getRoles().includes('Administrator');
  }

  public isSuperAdmin(): boolean {
    return this.getRoles().includes('SuperAdmin');
  }

  public getUserName(): string | null {
    return this.getDecodedTokenClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")[0] || null;
  }

}
