import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableComponent } from './components/table/table.component';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { ModalComponent } from './components/modal/modal.component';



@NgModule({
  declarations: [
    TableComponent,
    ModalComponent
  ],
  imports: [
    CommonModule,
    NgbPaginationModule
  ],
  exports:[
    TableComponent,
    ModalComponent
  ]
})
export class AdministratorModule { }
