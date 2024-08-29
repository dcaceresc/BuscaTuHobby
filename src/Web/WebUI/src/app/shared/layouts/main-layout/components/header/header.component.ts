import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBars, faHeart, faMagnifyingGlass, faSignInAlt, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { Observable } from 'rxjs';
import { LoginMenuComponent } from '../../../../../features/security/components/login-menu/login-menu.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterLink,FontAwesomeModule,LoginMenuComponent
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent { 

  public isAuthenticated : boolean = true;
  faMagnifyingGlass = faMagnifyingGlass;
  faSignInAlt = faSignInAlt;
  faUserPlus = faUserPlus;
  faBars = faBars;
  faHeart = faHeart;

  @ViewChild('btnCloseOffCanvas') btnCloseOffCanvas!: ElementRef;

  constructor() {
    
  }

  public onClose(){
    this.btnCloseOffCanvas.nativeElement.click();
  }

}
