import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GunplaComponent } from './pages/gunpla/gunpla.component';
import { UniverseComponent } from './pages/universe/universe.component';

const routes: Routes = [
  {path:'gunplas',component:GunplaComponent},
  {path:'universes',component:UniverseComponent}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdministratorRoutingModule { }
