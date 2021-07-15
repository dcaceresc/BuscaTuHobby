import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthenticationRoutingModule } from './authentication-routing.module';
import { LoginComponent } from './components/login/login.component';
import { LoginMenuComponent } from './components/login-menu/login-menu.component';
import { LogoutComponent } from './components/logout/logout.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';


@NgModule({
  declarations: [
    LoginComponent,
    LoginMenuComponent,
    LogoutComponent
  ],
  imports: [
    CommonModule,
    AuthenticationRoutingModule,
    FontAwesomeModule
  ],
  exports: [LoginMenuComponent, LoginComponent, LogoutComponent]
})
export class AuthenticationModule { }
