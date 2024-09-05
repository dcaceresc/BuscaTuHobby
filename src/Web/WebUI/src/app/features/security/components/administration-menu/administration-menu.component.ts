import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthorizeService } from '../../../../core/services/security/authorize.service';

@Component({
  selector: 'app-administration-menu',
  standalone: true,
  imports: [
    CommonModule,RouterLink,RouterLinkActive
  ],
  templateUrl: './administration-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdministrationMenuComponent { 

  private authorizeService = inject(AuthorizeService);
  @ViewChild('btnCloseOffCanvas') btnCloseOffCanvas!: ElementRef;

  public isSuperAdmin: boolean = false;


  constructor() {
    this.isSuperAdmin = this.authorizeService.isSuperAdmin();
  }

  public onClose(){
    this.btnCloseOffCanvas.nativeElement.click();
  }
}
