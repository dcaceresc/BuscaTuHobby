import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthorizeService } from '../services/security/auth.service';

export const AuthorizeGuard: CanActivateFn = (route, state) => {


  const router = inject(Router);
  const authorizeService = inject(AuthorizeService);


  const user = authorizeService.userValue;

  if (user) {
    // authorized so return true
    return true;
  }


  router.navigate([''], { queryParams: { returnUrl: state.url } });

  return false;
};
