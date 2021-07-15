import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdministratorRoutingModule } from './administrator-routing.module';
import { UniverseComponent } from './pages/universe/universe.component';
import { SerieComponent } from './pages/serie/serie.component';
import { ScaleComponent } from './pages/scale/scale.component';
import { ManufacturerComponent } from './pages/manufacturer/manufacturer.component';
import { GunplaComponent } from './pages/gunpla/gunpla.component';
import { GradeComponent } from './pages/grade/grade.component';
import { ListDataComponent } from './components/list-data/list-data.component';
import { NgxPaginationModule } from 'ngx-pagination';
import { OrderModule } from 'ngx-order-pipe';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';



@NgModule({
  declarations: [
    UniverseComponent,
    SerieComponent,
    ScaleComponent,
    ManufacturerComponent,
    GunplaComponent,
    GradeComponent,
    ListDataComponent,
  ],
  imports: [
    CommonModule,
    AdministratorRoutingModule,
    NgxPaginationModule,
    OrderModule,
    FontAwesomeModule,
  ]
})
export class AdministratorModule { }
