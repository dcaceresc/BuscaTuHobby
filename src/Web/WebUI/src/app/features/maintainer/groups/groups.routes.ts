import { Routes } from "@angular/router";

export const routes : Routes =[
    {path : '', loadComponent : () => import('./groups.component').then(m => m.GroupsComponent)}
];