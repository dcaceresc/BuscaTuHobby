import { Routes } from '@angular/router';

export const routes : Routes =[
    {path : '', loadComponent: () => import('./roles.component').then(m => m.RolesComponent)},
    {path: 'add', loadComponent: () => import('./add-role/add-role.component').then(m => m.AddRoleComponent)},
    {path: 'update/:id', loadComponent: () => import('./update-role/update-role.component').then(m => m.UpdateRoleComponent)}

]