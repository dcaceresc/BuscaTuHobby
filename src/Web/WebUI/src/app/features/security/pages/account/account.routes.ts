import { Routes } from "@angular/router";

export const routes: Routes = [
    {
        path : 'login',
        loadComponent: () => import('./user-login/user-login.component').then(m => m.UserLoginComponent),
      },
      {
        path: 'register',
        loadComponent: () => import('./register/register.component').then(m => m.RegisterComponent)
      },
      {
        path: 'admin',
        loadComponent: () => import('./admin-login/admin-login.component').then(m => m.AdminLoginComponent)
      },
];