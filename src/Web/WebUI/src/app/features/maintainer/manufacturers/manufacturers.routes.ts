import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./manufacturers.component').then(m => m.ManufacturersComponent)},
    {path: 'add', loadComponent: () => import('./add-edit-manufacturer/add-edit-manufacturer.component').then(m => m.AddEditManufacturerComponent)},
    {path: 'edit/:id', loadComponent: () => import('./add-edit-manufacturer/add-edit-manufacturer.component').then(m => m.AddEditManufacturerComponent)},

]