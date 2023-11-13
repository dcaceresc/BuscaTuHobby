import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { AuthorizeInterceptor } from './core/interceptors/authorize.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [provideRouter(routes),
    importProvidersFrom(BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }), FormsModule),
  { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
  provideHttpClient(withInterceptorsFromDi()),]
};
