import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { HomeModule } from './modules/home/home.module';
import { AuthenticationModule } from './modules/authentication/authentication.module';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthorizeInterceptor } from './core/interceptors/authorize.interceptor';
import { AdministratorModule } from './modules/administrator/administrator.module';
import { UniverseModule } from './modules/administrator/modules/universe/universe.module';
import { SerieModule } from './modules/administrator/modules/serie/serie.module';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    AppRoutingModule,
    CoreModule,
    SharedModule,
    HomeModule,
    AuthenticationModule,
    FontAwesomeModule,
    NgbModule,
    AdministratorModule,
    UniverseModule,
    SerieModule,
  ],
  providers: [{
    provide:HTTP_INTERCEPTORS, useClass : AuthorizeInterceptor, multi:true
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
