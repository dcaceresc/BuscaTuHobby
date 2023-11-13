import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { AuthorizeGuard } from './core/guards/authorize.guard';

export const routes: Routes = [
    { path: '', component: HomeComponent, pathMatch: 'full' },
    { path: '', loadChildren: () => import("./features/authentication/authentication.routes").then(m => m.routes), },
    { path: 'maintainer', loadChildren: () => import("./features/maintainer/maintainer.routes").then(m => m.routes), canActivate : [AuthorizeGuard]}
];
