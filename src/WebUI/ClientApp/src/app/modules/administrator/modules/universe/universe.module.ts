import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UniverseRoutingModule } from './universe-routing.module';
import { IndexComponent } from './pages/index/index.component';
import { CreateComponent } from './components/create/create.component';
import { AdministratorModule } from '../../administrator.module';


@NgModule({
  declarations: [
    IndexComponent,
    CreateComponent
  ],
  imports: [
    CommonModule,
    UniverseRoutingModule,
    AdministratorModule
  ]
})
export class UniverseModule { }
