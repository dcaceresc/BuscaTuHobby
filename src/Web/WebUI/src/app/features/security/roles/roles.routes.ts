import { Routes } from '@angular/router';

export const routes : Routes =[
    {path : '', loadComponent: () => import('./roles.component').then(m => m.RolesComponent)},
    {path: 'add', loadComponent: () => import('./add-edit-role/add-edit-role.component').then(m => m.AddEditRoleComponent)},
    {path: 'edit/:id', loadComponent: () => import('./add-edit-role/add-edit-role.component').then(m => m.AddEditRoleComponent)}

]