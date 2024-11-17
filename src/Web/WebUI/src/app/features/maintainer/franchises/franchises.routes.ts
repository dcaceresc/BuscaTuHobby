import { Routes } from "@angular/router";

export const routes : Routes = [
    {path : '', loadComponent : () => import('./franchises.component').then(m => m.FranchisesComponent)},
    {path : 'add', loadComponent : () => import('./add-edit-franchise/add-edit-franchise.component').then(m => m.AddEditFranchiseComponent)},
    {path : 'edit/:id', loadComponent : () => import('./add-edit-franchise/add-edit-franchise.component').then(m => m.AddEditFranchiseComponent)},
];