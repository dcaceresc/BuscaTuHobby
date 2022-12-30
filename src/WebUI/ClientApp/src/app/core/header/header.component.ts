import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs';
import { AuthenticationService } from '../authentication/authentication.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  public isMenuCollapsed = true;
  public isAuthenticated!: Observable<boolean>;

  constructor(private authenticationService: AuthenticationService) { }

  ngOnInit(): void {
    this.isAuthenticated = this.authenticationService.isAuthenticated();
  }

}
