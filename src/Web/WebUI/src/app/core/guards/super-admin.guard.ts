import { Router, type CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/security/auth.service';

// La API protege /api/security/roles con la política "SuperAdmin" y nada más, así que la pantalla de
// roles no puede abrirse con un Administrator: vería todas las llamadas rechazadas.
export const SuperAdminGuard: CanActivateFn = (route, state) => {

  const router = inject(Router);
  const authService = inject(AuthService);

  if (authService.isSuperAdmin()) {
    return true;
  }

  router.navigate(['/forbidden']);

  return false;
};
