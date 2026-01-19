import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '@app/core/services';
import { NgbDropdown, NgbDropdownToggle,	NgbDropdownMenu,	NgbDropdownItem,	NgbDropdownButtonItem,} from '@ng-bootstrap/ng-bootstrap/dropdown';

@Component({
    selector: 'app-login-menu',
    imports: [CommonModule, RouterLink,NgbDropdown, NgbDropdownToggle, NgbDropdownMenu, NgbDropdownItem, NgbDropdownButtonItem],
    templateUrl: './login-menu.component.html',
    styleUrl: './login-menu.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginMenuComponent {
  private authService = inject(AuthService);
    public isAuthenticated = signal<boolean>(false);

  constructor() {
    this.authService.isAuthenticated().subscribe((isAuthenticated: boolean) => {
      this.isAuthenticated.set(isAuthenticated);
    });
  }


  public logout() {
    this.authService.logout();
  }


}
