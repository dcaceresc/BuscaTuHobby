import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./configurations.component').then(m => m.ConfigurationsComponent)},
    {path: 'add', loadComponent: () => import('./add-configuration/add-configuration.component').then(m => m.AddConfigurationComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-configuration/update-configuration.component').then(m => m.UpdateConfigurationComponent)}
];