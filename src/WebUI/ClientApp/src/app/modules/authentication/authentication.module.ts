import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthenticationRoutingModule } from './authentication-routing.module';
import { LoginMenuComponent } from './components/login-menu/login-menu.component';


@NgModule({
  declarations: [
    LoginMenuComponent
  ],
  imports: [
    CommonModule,
    AuthenticationRoutingModule
  ],
  exports:[
    LoginMenuComponent
  ]
})
export class AuthenticationModule { }
