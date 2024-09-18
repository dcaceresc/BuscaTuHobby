import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./stores.component').then(m => m.StoresComponent)},
    {path: 'add', loadComponent: () => import('./add-store/add-store.component').then(m => m.AddStoreComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-store/update-store.component').then(m => m.UpdateStoreComponent)},
];