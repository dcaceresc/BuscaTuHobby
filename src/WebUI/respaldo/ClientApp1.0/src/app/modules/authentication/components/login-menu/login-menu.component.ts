import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { map,tap } from 'rxjs/operators';
import { AuthorizeService } from 'src/app/core/authentication/authentication.service';
import { faUser,faSignInAlt,faSignOutAlt } from '@fortawesome/free-solid-svg-icons';


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


  constructor(private authorizeService: AuthorizeService) { }

  ngOnInit() {
    this.isAuthenticated = this.authorizeService.isAuthenticated();
    this.userName = this.authorizeService.getUser().pipe(map(u => u && u.name));
  }

}
