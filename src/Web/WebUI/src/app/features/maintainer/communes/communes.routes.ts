import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./communes.component').then(m => m.CommunesComponent)},
    {path: 'add', loadComponent: () => import('./add-edit-commune/add-edit-commune.component').then(m => m.AddEditCommuneComponent)},
    {path: 'edit/:id', loadComponent: () => import('./add-edit-commune/add-edit-commune.component').then(m => m.AddEditCommuneComponent)},
];