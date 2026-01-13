import { ChangeDetectionStrategy, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { AuthService, DashboardService, NotificationService } from '@app/core/services';
import { Observable } from 'rxjs';
import { LoginMenuComponent } from './login-menu/login-menu.component';
import { AdministrationMenuComponent } from './administration-menu/administration-menu.component';
import { MenuComponent } from './menu/menu.component';
import { DashboardMenuDto } from '@app/core/models';

@Component({
    selector: 'app-header',
    imports: [LoginMenuComponent, AdministrationMenuComponent, MenuComponent],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent{ 

  readonly btnCloseOffCanvas = viewChild.required<ElementRef>('btnCloseOffCanvas');

  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);
  private notificationService = inject(NotificationService);

  public isAuthenticated!: Observable<boolean>;
  public roles = signal<string[]>([]);
  public menu = signal<DashboardMenuDto[]>([]);



  constructor() { 
    this.isAuthenticated = this.authService.isAuthenticated();

    this.isAuthenticated.subscribe((isAuthenticated: boolean) => {
      if (isAuthenticated === true) {
        this.roles.set(this.authService.getRoles() || []);
      }
    });

    this.dashboardService.getMenu().subscribe({
      next: (response) => {
        if(!response.success){
          return this.notificationService.showError("Error",response.message);
        }
        this.menu.set(response.data);
      },
      error: () => {
        return this.notificationService.showDefaultError();
      }
    })
  }

  public onClose(){
    this.btnCloseOffCanvas().nativeElement.click();
  }

}
