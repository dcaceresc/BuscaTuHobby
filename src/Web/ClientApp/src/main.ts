import { enableProdMode, importProvidersFrom } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';


import { environment } from './environments/environment';
import { AppComponent } from './app/app.component';
import { HomeComponent } from './app/features/home/home.component';
import { provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { AuthorizeInterceptor } from 'src/app/core/interceptors/authorize.interceptor';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { AuthorizeGuard } from './app/core/guards/authorize.guard';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
    providers: [
        importProvidersFrom(BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }), FormsModule),
        { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
        provideHttpClient(withInterceptorsFromDi()),
        provideRouter([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: '', loadChildren: () => import("./app/features/authentication/authentication.routes").then(m => m.routes), },
            { path: 'maintainer', loadChildren: () => import("./app/features/maintainer/maintainer.routes").then(m => m.routes), canActivate : [AuthorizeGuard]}
        ])
    ]
})
  .catch(err => console.log(err));
