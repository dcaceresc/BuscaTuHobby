import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPencil, faPlus, faPowerOff, IconDefinition } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [
    CommonModule,FontAwesomeModule
  ],
  template: `
  <button class="{{this.class}}">
    <fa-icon [icon]="faIcon"></fa-icon>
    {{this.text}}
  </button>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ButtonComponent { 

  @Input() class: string = "";
  @Input() text: string = "";
  @Input() icon: string = ""; 

  public faIcon! : IconDefinition 

  constructor() { 
    switch(this.icon){
      case "add":
        this.faIcon = faPlus;
        break;
      case "edit":
        this.faIcon = faPencil;
        break;
      case "toggle":
        this.faIcon = faPowerOff;
        break;
      default:
        this.faIcon = faPlus;
        break;
    }
  }

}