import { provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { ApplicationConfig, importProvidersFrom } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutingModule } from "./app.routes";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";

export const appConfig : ApplicationConfig = {
    providers: [
        importProvidersFrom(BrowserModule, AppRoutingModule, FontAwesomeModule),
        provideHttpClient(withInterceptorsFromDi())
    ]
};
    