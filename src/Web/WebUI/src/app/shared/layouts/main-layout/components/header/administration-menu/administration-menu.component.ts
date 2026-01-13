
import { ChangeDetectionStrategy, Component, ElementRef, input, viewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
    selector: 'app-administration-menu',
    imports: [
    RouterLink,
    RouterLinkActive,
],
    templateUrl: './administration-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdministrationMenuComponent { 

  readonly isSuperAdmin = input.required<boolean>(); 
  readonly btnCloseOffCanvas = viewChild.required<ElementRef>('btnCloseOffCanvas');

  public onClose(){
    this.btnCloseOffCanvas().nativeElement.click();
  }
}
