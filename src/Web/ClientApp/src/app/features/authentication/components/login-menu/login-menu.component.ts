import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthorizeService } from 'src/app/core/services/authorize.service';
import { Observable, map } from 'rxjs';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faSignInAlt, faUser, faUserPlus } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-login-menu',
  standalone: true,
  imports: [CommonModule,RouterLink,FontAwesomeModule],
  templateUrl: './login-menu.component.html',
  styleUrls: ['./login-menu.component.scss']
})
export class LoginMenuComponent implements OnInit {
  public isAuthenticated?: Observable<boolean>;
  public userName?: Observable<string | null | undefined>;
  public faSignInAlt = faSignInAlt;
  public faUserPlus = faUserPlus;
  public faUser = faUser;

  constructor(private authorizeService: AuthorizeService) { }

  ngOnInit() {
    this.isAuthenticated = this.authorizeService.isAuthenticated();
    this.userName = this.authorizeService.getUser().pipe(map(u => u && u.name));
  }
}
