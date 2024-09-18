import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./manufacturers.component').then(m => m.ManufacturersComponent)},
    {path: 'add', loadComponent: () => import('./add-manufacturer/add-manufacturer.component').then(m => m.AddManufacturerComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-manufacturer/update-manufacturer.component').then(m => m.UpdateManufacturerComponent)},

]