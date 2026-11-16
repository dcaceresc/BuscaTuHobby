import { Routes } from "@angular/router";
import { AdminGuard } from "../../core/guards/admin.guard";

export const routes : Routes =[
    {
        path: '',
        children: [
            {path: 'auth', loadChildren: () => import("./auth/auth.routes").then(m => m.routes)},
            {path: 'roles', canActivate: [AdminGuard], loadChildren: () => import("./roles/roles.routes").then(m => m.routes)},
            {path: 'users', canActivate: [AdminGuard], loadChildren: () => import("./users/users.routes").then(m => m.routes)},
        ]
    },
];
