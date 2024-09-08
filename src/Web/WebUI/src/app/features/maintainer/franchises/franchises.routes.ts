import { Routes } from "@angular/router";

export const routes : Routes = [
    {path : '', loadComponent : () => import('./franchises.component').then(m => m.FranchisesComponent)},
    {path : 'add', loadComponent : () => import('./add-franchise/add-franchise.component').then(m => m.AddFranchiseComponent)},
    {path : 'update/:id', loadComponent : () => import('./update-franchise/update-franchise.component').then(m => m.UpdateFranchiseComponent)}
];