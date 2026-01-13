import { provideHttpClient, withFetch, withInterceptors } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom, provideZonelessChangeDetection } from "@angular/core";
import { AuthorizeInterceptor } from "./core/interceptors/authorize.interceptor";
import { ErrorInterceptor } from "./core/interceptors/error.interceptor";
import { routes } from "./app.routes";
import { provideRouter, withComponentInputBinding } from "@angular/router";

export const appConfig : ApplicationConfig = {
    providers: [
        provideZonelessChangeDetection(),
        provideRouter(routes,withComponentInputBinding()),
        provideHttpClient(withFetch(),withInterceptors([AuthorizeInterceptor,ErrorInterceptor]))
    ]
};
    