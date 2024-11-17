import { provideHttpClient, withFetch, withInterceptors } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom, provideExperimentalZonelessChangeDetection } from "@angular/core";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";
import { AuthorizeInterceptor } from "./core/interceptors/authorize.interceptor";
import { ErrorInterceptor } from "./core/interceptors/error.interceptor";
import { routes } from "./app.routes";
import { provideRouter, withComponentInputBinding } from "@angular/router";

export const appConfig : ApplicationConfig = {
    providers: [
        provideExperimentalZonelessChangeDetection(),
        provideRouter(routes,withComponentInputBinding()),
        importProvidersFrom(FontAwesomeModule),
        provideHttpClient(withFetch(),withInterceptors([AuthorizeInterceptor,ErrorInterceptor]))
    ]
};
    