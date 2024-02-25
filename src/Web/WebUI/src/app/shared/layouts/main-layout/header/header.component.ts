import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBars, faHeart, faMagnifyingGlass, faSignInAlt, faUserPlus } from '@fortawesome/free-solid-svg-icons';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    RouterLink,FontAwesomeModule
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

  constructor() {
    
  }

}
