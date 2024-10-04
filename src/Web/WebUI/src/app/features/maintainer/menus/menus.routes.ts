import { Routes } from "@angular/router";

export const routes : Routes =[
    {path : '', loadComponent : () => import('./menus.component').then(m => m.MenusComponent)},
    {path : 'add', loadComponent : () => import('./add-menu/add-menu.component').then(m => m.AddMenuComponent)},
    {path : 'update/:id', loadComponent : () => import('./update-menu/update-menu.component').then(m => m.UpdateGroupComponent)},
];