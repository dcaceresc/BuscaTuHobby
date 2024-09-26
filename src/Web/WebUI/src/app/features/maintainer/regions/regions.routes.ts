import { Routes } from "@angular/router";

export const routes : Routes =[
    {path: '', loadComponent: () => import('./regions.component').then(m => m.RegionsComponent)},
    {path: 'add', loadComponent: () => import('./add-region/add-region.component').then(m => m.AddRegionComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-region/update-region.component').then(m => m.UpdateRegionComponent)}
];