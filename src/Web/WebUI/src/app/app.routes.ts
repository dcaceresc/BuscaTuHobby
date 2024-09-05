import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminGuard } from './core/guards/admin.guard';

const routes: Routes = [
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
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
