import { Routes } from "@angular/router";
import { LoginComponent } from "./pages/login/login.component";
import { ApplicationPaths } from "src/app/core/constants/authorization.constants";
import { LogoutComponent } from "./pages/logout/logout.component";

export const routes:Routes = [
    { path: ApplicationPaths.Register, component: LoginComponent },
    { path: ApplicationPaths.Profile, component: LoginComponent },
    { path: ApplicationPaths.Login, component: LoginComponent },
    { path: ApplicationPaths.LoginFailed, component: LoginComponent },
    { path: ApplicationPaths.LoginCallback, component: LoginComponent },
    { path: ApplicationPaths.LogOut, component: LogoutComponent },
    { path: ApplicationPaths.LoggedOut, component: LogoutComponent },
    { path: ApplicationPaths.LogOutCallback, component: LogoutComponent }

]
