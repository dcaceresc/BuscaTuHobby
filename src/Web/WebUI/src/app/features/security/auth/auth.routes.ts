import { Routes } from "@angular/router";

export const routes: Routes = [
  {
    path: 'admin',
    loadComponent: () => import('./admin-login/admin-login.component').then(m => m.AdminLoginComponent)
  },
  {
    path: 'confirm-email/:userId/:token',
    loadComponent: () => import('./confirm-email/confirm-email.component').then(m => m.ConfirmEmailComponent)
  }
];