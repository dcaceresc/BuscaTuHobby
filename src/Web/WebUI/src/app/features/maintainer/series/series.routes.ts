import { Routes } from "@angular/router";

export const routes : Routes = [
    {path: '', loadComponent: () => import('./series.component').then(m => m.SeriesComponent)},
    {path: 'add', loadComponent: () => import('./add-serie/add-serie.component').then(m => m.AddSerieComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-serie/update-serie.component').then(m => m.UpdateSerieComponent)}
];