import { ChangeDetectionStrategy, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthorizeService } from '@app/core/services';
import { AdministrationMenuComponent } from '@app/features/security/components/administration-menu/administration-menu.component';
import { LoginMenuComponent } from '@app/features/security/components/login-menu/login-menu.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBars, faHeart, faMagnifyingGlass, faSignInAlt, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterLink,FontAwesomeModule,LoginMenuComponent,AdministrationMenuComponent
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent{ 

  @ViewChild('btnCloseOffCanvas') btnCloseOffCanvas!: ElementRef;

  private authorizeService = inject(AuthorizeService);

  faMagnifyingGlass = faMagnifyingGlass;
  faSignInAlt = faSignInAlt;
  faUserPlus = faUserPlus;
  faBars = faBars;
  faHeart = faHeart;
  public isAuthenticated!: Observable<boolean>;
  public roles: string[] = [];

  constructor() { 
    this.isAuthenticated = this.authorizeService.isAuthenticated();

    this.isAuthenticated.subscribe((isAuthenticated: boolean) => {
      if (isAuthenticated === true) {
        this.roles = this.authorizeService.getRoles();
      }
    });
  }

  public onClose(){
    this.btnCloseOffCanvas.nativeElement.click();
  }

}
