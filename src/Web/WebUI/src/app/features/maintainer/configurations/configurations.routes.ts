import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./configurations.component').then(m => m.ConfigurationsComponent)},
    {path: 'add', loadComponent: () => import('./add-edit-configuration/add-edit-configuration.component').then(m => m.AddEditConfigurationComponent)},
    {path: 'edit/:id', loadComponent: () => import('./add-edit-configuration/add-edit-configuration.component').then(m => m.AddEditConfigurationComponent)},
];