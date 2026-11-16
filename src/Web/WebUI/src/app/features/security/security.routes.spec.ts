import { describe, expect, it } from 'vitest';
import type { CanActivateFn, Route } from '@angular/router';
import { routes } from './security.routes';
import { AdminGuard } from '../../core/guards/admin.guard';
import { SuperAdminGuard } from '../../core/guards/super-admin.guard';

// Las rutas hijas se leen del objeto de configuración: cada guardia debe coincidir con la política
// que la API aplica al mismo recurso, aunque el componente no se renderice aquí.
function children(): Route[] {
  const shell = routes[0];
  return (shell.children ?? []) as Route[];
}

function routeByPath(path: string): Route {
  const route = children().find(child => child.path === path);

  if (!route) {
    throw new Error(`La ruta ${path} no está declarada en security.routes`);
  }

  return route;
}

function guardsOf(path: string): CanActivateFn[] {
  const canActivate = routeByPath(path).canActivate ?? [];
  return canActivate as CanActivateFn[];
}

describe('security.routes', () => {
  it('guards roles with the SuperAdmin guard because RoleModule requires SuperAdmin only', () => {
    expect(guardsOf('roles')).toEqual([SuperAdminGuard]);
    expect(guardsOf('roles')).not.toContain(AdminGuard);
  });

  it('keeps users on AdminGuard because UserModule accepts Administrator or SuperAdmin', () => {
    expect(guardsOf('users')).toEqual([AdminGuard]);
    expect(guardsOf('users')).not.toContain(SuperAdminGuard);
  });

  it('leaves auth unguarded so anonymous login and confirm-email stay reachable', () => {
    const route = routeByPath('auth');

    expect(route.canActivate ?? []).toEqual([]);
    expect(route.canLoad ?? []).toEqual([]);
    expect(route.canMatch ?? []).toEqual([]);
  });

  it('declares exactly the three child paths the security shell serves', () => {
    expect(children().map(child => child.path)).toEqual(['auth', 'roles', 'users']);
  });
});
