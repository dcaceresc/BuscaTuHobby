import { Routes } from "@angular/router";

export const routes : Routes =[
    {path : '', loadComponent: () => import('./users.component').then(m => m.UsersComponent)},
    {path : 'add', loadComponent: () => import('./add-user/add-user.component').then(m => m.AddUserComponent)},
    {path : 'update/:id', loadComponent: () => import('./update-user/update-user.component').then(m => m.UpdateUserComponent)}
]