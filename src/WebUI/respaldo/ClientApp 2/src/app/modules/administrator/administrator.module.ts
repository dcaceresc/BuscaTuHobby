import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdministratorRoutingModule } from './administrator-routing.module';
import { GunplaComponent } from './pages/gunpla/gunpla.component';
import { UniverseComponent } from './pages/universe/universe.component';
import { GradeComponent } from './pages/grade/grade.component';
import { ManufacturerComponent } from './pages/manufacturer/manufacturer.component';
import { ScaleComponent } from './pages/scale/scale.component';
import { SerieComponent } from './pages/serie/serie.component';


@NgModule({
  declarations: [
    GunplaComponent,
    UniverseComponent,
    GradeComponent,
    ManufacturerComponent,
    ScaleComponent,
    SerieComponent
  ],
  imports: [
    CommonModule,
    AdministratorRoutingModule
  ]
})
export class AdministratorModule { }
