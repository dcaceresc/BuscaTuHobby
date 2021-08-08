import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdministratorRoutingModule } from './administrator-routing.module';
import { UniversesComponent } from './pages/universes/universes.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { TableComponent } from './components/table/table.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { ModalUniverseComponent } from './components/modal-universe/modal-universe.component';
import { ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    UniversesComponent,
    TableComponent,
    ModalUniverseComponent,
  ],
  imports: [
    CommonModule,
    AdministratorRoutingModule,
    NgbModule,
    FontAwesomeModule,
    ReactiveFormsModule
  ]
})
export class AdministratorModule { }
