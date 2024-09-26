import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./communes.component').then(m => m.CommunesComponent)},
    {path: 'add', loadComponent: () => import('./add-commune/add-commune.component').then(m => m.AddCommuneComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-commune/update-commune.component').then(m => m.UpdateCommuneComponent)}
];