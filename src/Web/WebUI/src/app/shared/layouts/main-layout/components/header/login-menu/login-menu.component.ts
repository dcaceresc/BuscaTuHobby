import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '@app/core/services';

@Component({
    selector: 'app-login-menu',
    imports: [CommonModule, RouterLink],
    templateUrl: './login-menu.component.html',
    styleUrl: './login-menu.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginMenuComponent {

  readonly isAuthenticated = input.required<Observable<boolean>>();

  private authService = inject(AuthService);


  public userName = signal<string | null | undefined>(null);


  public logout() {
    this.authService.logout();
  }


}
