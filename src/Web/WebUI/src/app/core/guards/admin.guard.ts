import { Router, type CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/security/auth.service';

export const AdminGuard: CanActivateFn = (route, state) => {

  const router = inject(Router);
  const authService = inject(AuthService);


  if(authService.isAdmin() || authService.isSuperAdmin()) {
    return true;
  }

  router.navigate(['/forbidden']);

  return false;
};
