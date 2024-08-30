import { Routes } from "@angular/router";

export const routes : Routes =[
    {path: 'account', loadChildren: () => import("./pages/account/account.routes").then(m => m.routes)},
    {path: 'roles', loadChildren: () => import("./pages/roles/roles.routes").then(m => m.routes)},
    {path: 'users', loadChildren: () => import("./pages/users/users.routes").then(m => m.routes)},
];	