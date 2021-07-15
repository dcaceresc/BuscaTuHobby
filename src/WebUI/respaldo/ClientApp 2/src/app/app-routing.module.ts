import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthorizeGuard } from './core/guards/authorize.guard';

const homeModule = () => import('./modules/home/home.module').then(x => x.HomeModule);
const administratorModule = () => import('./modules/administrator/administrator.module').then(x=>x.AdministratorModule)

const routes: Routes = [
      { path: '', loadChildren: homeModule , pathMatch: 'full' },
      { path: 'administrator',loadChildren: administratorModule, canActivate: [AuthorizeGuard]},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
