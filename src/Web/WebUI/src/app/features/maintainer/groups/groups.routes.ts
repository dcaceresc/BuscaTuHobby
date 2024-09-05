import { Routes } from "@angular/router";

export const routes : Routes =[
    {path : '', loadComponent : () => import('./groups.component').then(m => m.GroupsComponent)},
    {path : 'add', loadComponent : () => import('./add-group/add-group.component').then(m => m.AddGroupComponent)},
    {path : 'update/:id', loadComponent : () => import('./update-group/update-group.component').then(m => m.UpdateGroupComponent)},
];