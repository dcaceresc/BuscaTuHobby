import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { AdministratorModule } from './modules/administrator/administrator.module';
import { HomeModule } from './modules/home/home.module';
import { CoreModule } from './core/core.module';

const homeModule = () => import('./modules/home/home.module').then(x => x.HomeModule);
const administratorModule = () => import('./modules/administrator/administrator.module').then(x=>x.AdministratorModule)

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ApiAuthorizationModule,
    RouterModule.forRoot([
      { path: '', loadChildren: homeModule , pathMatch: 'full' },
      { path: 'administrator',loadChildren: administratorModule, canActivate: [AuthorizeGuard]},
    ]),
    AdministratorModule,
    HomeModule,
    CoreModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
