import { ChangeDetectionStrategy, Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardMenuDto } from '@app/core/models';
import { AuthorizeService, DashboardService, FaIconService, NotificationService } from '@app/core/services';
import { AdministrationMenuComponent } from '@app/features/security/components/administration-menu/administration-menu.component';
import { LoginMenuComponent } from '@app/features/security/components/login-menu/login-menu.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [FontAwesomeModule,LoginMenuComponent,AdministrationMenuComponent],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent{ 

  @ViewChild('btnCloseOffCanvas') btnCloseOffCanvas!: ElementRef;

  private authorizeService = inject(AuthorizeService);
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
    this.isAuthenticated = this.authorizeService.isAuthenticated();

    this.isAuthenticated.subscribe((isAuthenticated: boolean) => {
      if (isAuthenticated === true) {
        this.roles = this.authorizeService.getRoles();
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
    this.btnCloseOffCanvas.nativeElement.click();
  }

}
