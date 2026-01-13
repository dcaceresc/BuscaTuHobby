import { Routes } from '@angular/router';
import { AdminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./shared/layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      { 
        path: 'maintainer', 
        loadChildren: () => import("./features/maintainer/maintainer.routes").then(m => m.routes),
        canActivate:[AdminGuard]
      },
      {
        path: 'security',
        loadChildren: () => import("./features/security/security.routes").then(m => m.routes)
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./shared/components/forbidden/forbidden.component').then(m => m.ForbiddenComponent)
      }
    ]
  },
  { path: 'login', 
    loadComponent: () => import('./features/security/auth/user-login/user-login.component').then(m => m.UserLoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/security/auth/register/register.component').then(m => m.RegisterComponent),
  }
];