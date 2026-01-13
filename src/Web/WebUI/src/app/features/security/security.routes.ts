import { Routes } from "@angular/router";

export const routes : Routes =[
    {path: 'auth', loadChildren: () => import("./auth/auth.routes").then(m => m.routes)},
    {path: 'roles', loadChildren: () => import("./roles/roles.routes").then(m => m.routes)},
    {path: 'users', loadChildren: () => import("./users/users.routes").then(m => m.routes)},
];	