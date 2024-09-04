import type { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthorizeService } from '../services/security/authorize.service';

export const ErrorInterceptor: HttpInterceptorFn = (req, next) => {

  const authorizeService = inject(AuthorizeService);
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
