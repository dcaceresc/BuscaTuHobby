import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal, input } from '@angular/core';
import { faSignInAlt, faSignOutAlt, faUser, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { Observable } from 'rxjs';
import { AuthorizeService } from '@app/core/services';

@Component({
    selector: 'app-login-menu',
    imports: [CommonModule, RouterLink, FontAwesomeModule],
    templateUrl: './login-menu.component.html',
    styleUrl: './login-menu.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginMenuComponent {

  readonly isAuthenticated = input.required<Observable<boolean>>();

  private authorizeService = inject(AuthorizeService);


  public userName = signal<string | null | undefined>(null);
  public faSignInAlt = faSignInAlt;
  public faSignOutAlt = faSignOutAlt;
  public faUserPlus = faUserPlus;
  public faUser = faUser;

  public logout() {
    this.authorizeService.logout();
  }


}
