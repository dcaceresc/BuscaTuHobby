import { Routes } from '@angular/router';
import { AdminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  // login y register se declaran antes del shell del layout: el shell es dueño de la ruta '**', así que
  // de otro modo respondería /login y /register con la página de no encontrado.
  { path: 'login', 
    loadComponent: () => import('./features/security/auth/user-login/user-login.component').then(m => m.UserLoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/security/auth/register/register.component').then(m => m.RegisterComponent),
  },
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
        path: 'products/:productId',
        loadComponent: () => import('./features/dashboard/product-detail/product-detail.component').then(m => m.ProductDetailComponent)
      },
      {
        path: 'categories/:slug',
        loadComponent: () => import('./features/catalog/catalog.component').then(m => m.CatalogComponent)
      },
      {
        path: 'stores/:slug',
        loadComponent: () => import('./features/stores/store.component').then(m => m.StoreComponent)
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./shared/components/forbidden/forbidden.component').then(m => m.ForbiddenComponent)
      },
      {
        path: '**',
        loadComponent: () => import('./features/not-found/not-found.component').then(m => m.NotFoundComponent)
      }
    ]
  },
];
