import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { HomeModule } from './modules/home/home.module';
import { AdministratorModule } from './modules/administrator/administrator.module';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { AuthenticationModule } from './modules/authentication/authentication.module';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthorizeInterceptor } from './core/interceptors/authorize.interceptor';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CoreModule,
    SharedModule,
    HomeModule,
    AdministratorModule,
    NgbModule,
    FontAwesomeModule,
    AuthenticationModule
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
