import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ApplicationPaths } from 'src/app/shared/models/api-authorization.constants';
import { LoginComponent } from './components/login/login.component';
import { LogoutComponent } from './components/logout/logout.component';

const routes: Routes = [
  { path: ApplicationPaths.Register, component: LoginComponent },
  { path: ApplicationPaths.Profile, component: LoginComponent },
  { path: ApplicationPaths.Login, component: LoginComponent },
  { path: ApplicationPaths.LoginFailed, component: LoginComponent },
  { path: ApplicationPaths.LoginCallback, component: LoginComponent },
  { path: ApplicationPaths.LogOut, component: LogoutComponent },
  { path: ApplicationPaths.LoggedOut, component: LogoutComponent },
  { path: ApplicationPaths.LogOutCallback, component: LogoutComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthenticationRoutingModule { }
