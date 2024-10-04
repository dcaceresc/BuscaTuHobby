import { Routes } from "@angular/router";

export const routes: Routes = [
    {path: '', loadComponent: () => import('./submenus.component').then(m => m.ScalesComponent)},
    {path: 'add', loadComponent: () => import('./add-submenu/add-submenu.component').then(m => m.AddScaleComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-submenu/update-submenu.component').then(m => m.UpdateScaleComponent)},
];