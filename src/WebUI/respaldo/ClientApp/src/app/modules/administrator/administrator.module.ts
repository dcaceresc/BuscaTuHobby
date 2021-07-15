import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdministratorRoutingModule } from './administrator-routing.module';
import { GunplaComponent } from './pages/gunpla/gunpla.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UniverseComponent } from './pages/universe/universe.component';
import { GradeComponent } from './pages/grade/grade.component';
import { ScaleComponent } from './pages/scale/scale.component';
import { SerieComponent } from './pages/serie/serie.component';
import { ManufacturerComponent } from './pages/manufacturer/manufacturer.component';


@NgModule({
  declarations: [GunplaComponent, UniverseComponent, GradeComponent, ScaleComponent, SerieComponent, ManufacturerComponent],
  imports: [
    CommonModule,
    AdministratorRoutingModule,
    FormsModule,
    ReactiveFormsModule
  ],
})
export class AdministratorModule { }
