import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUserTie } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-administration-menu',
  standalone: true,
  imports: [
    CommonModule,RouterLink,RouterLinkActive,FontAwesomeModule
  ],
  templateUrl: './administration-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdministrationMenuComponent { 

  @Input() isSuperAdmin! :boolean; 
  @ViewChild('btnCloseOffCanvas') btnCloseOffCanvas!: ElementRef;

  public adminIcon = faUserTie;

  public onClose(){
    this.btnCloseOffCanvas.nativeElement.click();
  }
}
