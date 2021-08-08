import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GradeComponent } from './pages/grade/grade.component';
import { GunplaComponent } from './pages/gunpla/gunpla.component';
import { ManufacturerComponent } from './pages/manufacturer/manufacturer.component';
import { ScaleComponent } from './pages/scale/scale.component';
import { SerieComponent } from './pages/serie/serie.component';
import { UniverseComponent } from './pages/universe/universe.component';

const routes: Routes = [
  {path:'grades',component:GradeComponent},
  {path:'gunplas',component:GunplaComponent},
  {path:'manufacturers',component:ManufacturerComponent},
  {path:'scales',component:ScaleComponent},
  {path:'series',component:SerieComponent},
  {path:'universes',component:UniverseComponent}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdministratorRoutingModule { }
