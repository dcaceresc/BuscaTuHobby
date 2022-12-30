import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdministratorRoutingModule } from './administrator-routing.module';
import { TableComponent } from './components/table/table.component';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';


@NgModule({
  declarations: [
    TableComponent
  ],
  imports: [
    CommonModule,
    AdministratorRoutingModule,
    NgbPaginationModule
  ],
  exports:[
    TableComponent
  ]
})
export class AdministratorModule { }
