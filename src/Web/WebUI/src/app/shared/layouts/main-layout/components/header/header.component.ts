import { ChangeDetectionStrategy, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { AuthService } from '@app/core/services';
import { Observable } from 'rxjs';
import { LoginMenuComponent } from './login-menu/login-menu.component';
import { AdministrationMenuComponent } from './administration-menu/administration-menu.component';
import { NgbOffcanvas } from '@ng-bootstrap/ng-bootstrap/offcanvas';
import { MenuComponent } from './menu/menu.component';


@Component({
    selector: 'app-header',
    imports: [LoginMenuComponent ],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent{ 

  private authService = inject(AuthService);
  private offcanvasService = inject(NgbOffcanvas)

  public isAuthenticated!: Observable<boolean>;
  public roles = signal<string[]>([]);

  public isSuperAdmin() {
    return this.authService.isSuperAdmin();
  }

  public isAdmin() {
    return this.authService.isAdmin();
  }

  public open() {
		this.offcanvasService.open(MenuComponent);
	}

  public openAdminMenu() {
    this.offcanvasService.open(AdministrationMenuComponent, {
      position: 'end',
    });
  }

}
