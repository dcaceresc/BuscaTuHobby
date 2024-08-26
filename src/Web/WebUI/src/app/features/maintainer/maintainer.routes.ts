import { Routes } from "@angular/router";

export const routes : Routes =[
    
    { path: 'categories', loadChildren: () => import("./categories/categories.routes").then(m => m.routes) },
    { path: 'groups', loadChildren: () => import("./groups/groups.routes").then(m => m.routes) },
];