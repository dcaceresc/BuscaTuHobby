import { Routes } from "@angular/router";

export const routes : Routes =[
    {path : '', loadComponent: () => import('./users.component').then(m => m.UsersComponent)},
    {path : 'add', loadComponent: () => import('./add-edit-user/add-edit-user.component').then(m => m.AddEditUserComponent)},
    {path : 'edit/:id', loadComponent: () => import('./add-edit-user/add-edit-user.component').then(m => m.AddEditUserComponent)}
]