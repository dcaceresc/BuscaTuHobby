import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, inject, Input, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthorizeService } from '../../../../core/services/security/authorize.service';
import { Observable } from 'rxjs';

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

  @Input() isSuperAdmin! :boolean; 
  @ViewChild('btnCloseOffCanvas') btnCloseOffCanvas!: ElementRef;

  public onClose(){
    this.btnCloseOffCanvas.nativeElement.click();
  }
}
