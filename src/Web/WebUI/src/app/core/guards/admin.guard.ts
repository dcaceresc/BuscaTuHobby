import { Router, type CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/security/auth.service';

// Cubre lo que la API deja pasar a un Administrator: las pantallas de usuarios y de mantenedores.
// Las rutas que la API reserva a SuperAdmin usan SuperAdminGuard, en este mismo directorio.
export const AdminGuard: CanActivateFn = (route, state) => {

  const router = inject(Router);
  const authService = inject(AuthService);


  if(authService.isAdmin() || authService.isSuperAdmin()) {
    return true;
  }

  router.navigate(['/forbidden']);

  return false;
};
