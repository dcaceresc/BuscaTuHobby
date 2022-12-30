import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {path:'universes',loadChildren: () => import('./modules/universe/universe.module').then(m => m.UniverseModule)},
  {path:'series',loadChildren: () => import('./modules/serie/serie.module').then(m => m.SerieModule)}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdministratorRoutingModule { }
