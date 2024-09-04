import { provideHttpClient, withInterceptors, withInterceptorsFromDi } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutingModule } from "./app.routes";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";
import { AuthorizeInterceptor } from "./core/interceptors/authorize.interceptor";
import { ErrorInterceptor } from "./core/interceptors/error.interceptor";

export const appConfig : ApplicationConfig = {
    providers: [
        importProvidersFrom(BrowserModule, AppRoutingModule, FontAwesomeModule),
        provideHttpClient(withInterceptorsFromDi(),withInterceptors([AuthorizeInterceptor,ErrorInterceptor]))
    ]
};
    