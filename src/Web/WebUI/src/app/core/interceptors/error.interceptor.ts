import type { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/security/auth.service';

export const ErrorInterceptor: HttpInterceptorFn = (req, next) => {

  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(catchError(err => {
    if(err.status === 403) {
      router.navigate(['/forbidden']);
    }

    // if(err.status !== 401){
    //   const error = err.error || err.statusText;
    //   router.navigate(['/error'], { queryParams: { error } });
    //   return throwError(() => error);
    // }

    return throwError(() => err);
  }));
};
