import { ChangeDetectionStrategy, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { AuthService, DashboardService, FaIconService, NotificationService } from '@app/core/services';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { Observable } from 'rxjs';
import { LoginMenuComponent } from './login-menu/login-menu.component';
import { AdministrationMenuComponent } from './administration-menu/administration-menu.component';
import { MenuComponent } from './menu/menu.component';
import { DashboardMenuDto } from '@app/core/models';

@Component({
    selector: 'app-header',
    imports: [FontAwesomeModule, LoginMenuComponent, AdministrationMenuComponent, MenuComponent],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent{ 

  readonly btnCloseOffCanvas = viewChild.required<ElementRef>('btnCloseOffCanvas');

  private authService = inject(AuthService);
  private dashboardService = inject(DashboardService);
  private faIconService = inject(FaIconService);
  private notificationService = inject(NotificationService);

  public searchIcon!: IconDefinition
  public logoutIcon!: IconDefinition;
  public menuIcon!: IconDefinition;
  public favoriteIcon!: IconDefinition;

  public isAuthenticated!: Observable<boolean>;
  public roles: string[] = [];
  public menu = signal<DashboardMenuDto[]>([]);



  constructor() { 
    this.isAuthenticated = this.authService.isAuthenticated();

    this.isAuthenticated.subscribe((isAuthenticated: boolean) => {
      if (isAuthenticated === true) {
        this.roles = this.authService.getRoles();
      }
    });

    this.searchIcon = this.faIconService.getIcon('Search');
    this.logoutIcon = this.faIconService.getIcon('Logout');
    this.menuIcon = this.faIconService.getIcon('Bars');
    this.favoriteIcon = this.faIconService.getIcon('Heart');



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
