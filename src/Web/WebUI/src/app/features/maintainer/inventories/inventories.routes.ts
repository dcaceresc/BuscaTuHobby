import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./inventories.component').then(m => m.InventoriesComponent)},
    {path: 'add', loadComponent: () => import('./add-edit-inventory/add-edit-inventory.component').then(m => m.AddEditInventoryComponent)},
    {path: 'edit/:id', loadComponent: () => import('./add-edit-inventory/add-edit-inventory.component').then(m => m.AddEditInventoryComponent)},
];