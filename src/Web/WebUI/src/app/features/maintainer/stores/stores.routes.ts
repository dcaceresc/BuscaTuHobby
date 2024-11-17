import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./stores.component').then(m => m.StoresComponent)},
    {path: 'add', loadComponent: () => import('./add-edit-store/add-edit-store.component').then(m => m.AddEditStoreComponent)},
    {path: 'edit/:id', loadComponent: () => import('./add-edit-store/add-edit-store.component').then(m => m.AddEditStoreComponent)},
];