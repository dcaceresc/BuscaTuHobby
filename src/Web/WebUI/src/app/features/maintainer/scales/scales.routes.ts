import { Routes } from "@angular/router";

export const routes: Routes = [
    {path: '', loadComponent: () => import('./scales.component').then(m => m.ScalesComponent)},
    {path: 'add', loadComponent: () => import('./add-scale/add-scale.component').then(m => m.AddScaleComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-scale/update-scale.component').then(m => m.UpdateScaleComponent)},
];