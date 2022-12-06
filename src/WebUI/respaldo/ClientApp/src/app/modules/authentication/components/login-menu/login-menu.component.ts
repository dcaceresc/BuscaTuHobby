import { Component, OnInit } from '@angular/core';
import { faSignInAlt, faSignOutAlt, faUser } from '@fortawesome/free-solid-svg-icons';
import { Observable } from 'rxjs';
import { map} from 'rxjs/operators';
import { AuthenticationService } from 'src/app/core/authentication/authentication.service';

@Component({
  selector: 'app-login-menu',
  templateUrl: './login-menu.component.html',
  styleUrls: ['./login-menu.component.scss']
})
export class LoginMenuComponent implements OnInit {

  public isAuthenticated!: Observable<boolean>;
  public userName!: Observable<string | null | undefined>;
  faUser = faUser;
  faSignInAlt = faSignInAlt;
  faSignOutAlt = faSignOutAlt;

  constructor(private authenticationService: AuthenticationService) { }

  ngOnInit(): void {
    this.isAuthenticated = this.authenticationService.isAuthenticated();
    this.userName = this.authenticationService.getUser().pipe(map(u => u && u.name));
  }

}
