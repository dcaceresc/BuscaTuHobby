import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./inventories.component').then(m => m.InventoriesComponent)},
    {path: 'add', loadComponent: () => import('./add-inventory/add-inventory.component').then(m => m.AddInventoryComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-inventory/update-inventory.component').then(m => m.UpdateInventoryComponent)},
];