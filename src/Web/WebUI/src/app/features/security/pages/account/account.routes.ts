import { Routes } from '@angular/router';

export const routes : Routes =[
    {path : '', loadComponent: () => import('./account.component').then(m => m.AccountComponent)},
    {path : 'login', loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)}
]