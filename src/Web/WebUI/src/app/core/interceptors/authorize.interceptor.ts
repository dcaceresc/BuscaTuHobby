import { HttpRequest, type HttpInterceptorFn, HttpHandlerFn } from '@angular/common/http';
import { AuthService } from '../services/security/auth.service';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';


const addToken = (req: HttpRequest<any>, token: string) =>{
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });
}

const handleUnauthorizedError = (req: HttpRequest<any>, next: HttpHandlerFn, authorizeService : AuthService) =>{

  return authorizeService.refreshToken().pipe(
    switchMap((response) => {
      if (response.success && response.data) {
        // Token de actualización exitoso, volver a intentar la solicitud con el nuevo token de acceso
        authorizeService.userValue = response.data;
        req = addToken(req, response.data.accessToken);
        return next(req);
      } else {
        // No se pudo renovar el token de acceso, redirigir a la página de inicio de sesión
        authorizeService.logout();
        return throwError(() => "Usted no está autorizado, favor comunicarse con el administrador.");
      }
    }),
    catchError(() => {
      // Manejar cualquier error inesperado, redirigir a la página de inicio de sesión
      authorizeService.logout();
      return throwError(() => "Usted no está autorizado, favor comunicarse con el administrador.");
    })
  );
}


export const AuthorizeInterceptor: HttpInterceptorFn = (req, next) => {

  const authService = inject(AuthService);
  const accessToken = authService.userValue?.accessToken;


  if (accessToken) {
    req = addToken(req, accessToken);
  }


  return next(req).pipe(
    catchError((error) => {
      if (error.status === 401) {
        // Token de acceso expirado, intentar renovar
        return handleUnauthorizedError(req, next,authService);
      }
      return throwError(() => error);
    })
  );
};




