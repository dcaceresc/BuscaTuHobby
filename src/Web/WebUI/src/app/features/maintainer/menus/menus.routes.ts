import { Routes } from "@angular/router";

export const routes : Routes =[
    {path : '', loadComponent : () => import('./menus.component').then(m => m.MenusComponent)},
    {path : 'add', loadComponent : () => import('./add-edit-menu/add-edit-menu.component').then(m => m.AddEditMenuComponent)},
    {path : 'edit/:id', loadComponent : () => import('./add-edit-menu/add-edit-menu.component').then(m => m.AddEditMenuComponent)},
];