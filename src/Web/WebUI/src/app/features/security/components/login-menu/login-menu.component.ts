import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, Input, signal } from '@angular/core';
import { AuthorizeService } from '../../../../core/services/security/authorize.service';
import { faSignInAlt, faSignOutAlt, faUser, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-login-menu',
  standalone: true,
  imports: [
    CommonModule,RouterLink,FontAwesomeModule
  ],
  templateUrl: './login-menu.component.html',
  styleUrl: './login-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginMenuComponent {

  @Input() isAuthenticated! : Observable<boolean>;


  public userName = signal<string | null | undefined>(null);
  public faSignInAlt = faSignInAlt;
  public faSignOutAlt = faSignOutAlt;
  public faUserPlus = faUserPlus;
  public faUser = faUser;


 }
