import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, input, viewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUserTie } from '@fortawesome/free-solid-svg-icons';

@Component({
    selector: 'app-administration-menu',
    imports: [
        CommonModule, RouterLink, RouterLinkActive, FontAwesomeModule
    ],
    templateUrl: './administration-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdministrationMenuComponent { 

  readonly isSuperAdmin = input.required<boolean>(); 
  readonly btnCloseOffCanvas = viewChild.required<ElementRef>('btnCloseOffCanvas');

  public adminIcon = faUserTie;

  public onClose(){
    this.btnCloseOffCanvas().nativeElement.click();
  }
}
