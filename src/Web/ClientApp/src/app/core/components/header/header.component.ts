import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LoginMenuComponent } from 'src/app/features/authentication/components/login-menu/login-menu.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {  faMagnifyingGlass,faSignInAlt, faUserPlus, faBars } from '@fortawesome/free-solid-svg-icons';
import { AuthorizeService } from '../../services/authorize.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule,RouterModule,LoginMenuComponent, FontAwesomeModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit{
  public isAuthenticated?: Observable<boolean>;
  faMagnifyingGlass = faMagnifyingGlass;
  faSignInAlt = faSignInAlt;
  faUserPlus = faUserPlus;
  faBars = faBars;

  constructor(private authorizeService: AuthorizeService){}

  ngOnInit(): void {
    this.isAuthenticated = this.authorizeService.isAuthenticated();
  }

}
