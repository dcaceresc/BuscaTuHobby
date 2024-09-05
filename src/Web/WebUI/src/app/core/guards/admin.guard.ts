import { Router, type CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthorizeService } from '../services/security/authorize.service';

export const AdminGuard: CanActivateFn = (route, state) => {

  const router = inject(Router);
  const authorizeService = inject(AuthorizeService);


  if(authorizeService.isAdmin() || authorizeService.isSuperAdmin()) {
    return true;
  }

  router.navigate(['/forbidden']);

  return false;
};
