import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UniversesComponent } from './pages/universes/universes.component';

const routes: Routes = [
  {path:'universes',component:UniversesComponent}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdministratorRoutingModule { }
