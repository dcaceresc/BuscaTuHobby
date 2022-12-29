import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UniverseRoutingModule } from './universe-routing.module';
import { IndexComponent } from './pages/index/index.component';
import { AdministratorModule } from '../../administrator.module';
import { NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { CreateComponent } from './components/create/create.component';
import { UpdateComponent } from './components/update/update.component';


@NgModule({
  declarations: [
    IndexComponent,
    CreateComponent,
    UpdateComponent
  ],
  imports: [
    CommonModule,
    UniverseRoutingModule,
    AdministratorModule,
    NgbModalModule
  ]
})
export class UniverseModule { }
